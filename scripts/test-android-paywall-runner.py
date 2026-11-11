#!/usr/bin/env python3
"""Exercise the runner using isolated fake CLIs; never connects to a device.

Covers successful portrait/landscape captures, a broken CTA, absent reveal,
absent hide, optional transient diagnostics, JUnit, and settings restoration.
"""
import os
from pathlib import Path
import subprocess, tempfile
import xml.etree.ElementTree as ET
root=Path(__file__).resolve().parents[1]
with tempfile.TemporaryDirectory() as tmp:
    tmp=Path(tmp)
    fake='''#!/usr/bin/env python3
import json, os, sys
from pathlib import Path
state=Path(os.environ['FAKE_STATE']); args=sys.argv[1:]
with (state/'calls').open('a') as f: f.write(Path(sys.argv[0]).name+' '+str(args)+'\\n')
rotation=(state/'rotation').read_text() if (state/'rotation').exists() else '2'
if Path(sys.argv[0]).name=='adb':
    args=args[3:]
    if args[:3]==['settings','get','system']: print('1' if args[3]=='accelerometer_rotation' else rotation)
    elif args==['wm','fixed-to-user-rotation']: print('default')
    elif args==['wm','user-rotation']: print('lock 2')
    elif args[:3]==['wm','user-rotation','lock']:
        if (state/'fixed').exists() and (state/'fixed').read_text()=='enabled': (state/'rotation').write_text(args[3])
    elif args[:2]==['wm','fixed-to-user-rotation']: (state/'fixed').write_text(args[2])
    elif args[:3]==['cmd','overlay','list']: print('[x] com.android.internal.systemui.navbar.gestural\\n[ ] com.android.internal.systemui.navbar.threebutton')
    elif args[:2]==['input','swipe']:
        expected=['540','2399','540','1920','300'] if rotation=='0' else ['2399','540','1920','540','300']
        assert args[2:]==expected, args
        (state/'transient').write_text('visible')
    elif args==['dumpsys','window','displays']:
        print('mDisplayFrame=Rect(0, 0 - '+('1080, 2400)' if rotation=='0' else '2400, 1080)'))
        visible=(state/'transient').exists() and (state/'transient').read_text()=='visible'
        if os.environ.get('NO_REVEAL'): visible=False
        if visible and not os.environ.get('NO_TRANSIENT'): print('mShowingTransientTypes=navigationBars')
        print('InsetsSource type=navigationBars frame='+('[0,2274][1080,2400]' if rotation=='0' else '[2274,0][2400,1080]')+' visible='+str(visible).lower())
        if visible and not os.environ.get('NO_HIDE'): (state/'transient').write_text('hidden')
elif 'hierarchy' in args:
    footer='[100,2100][980,2274]' if rotation=='0' else '[100,900][2200,1000]'
    cta='[100,1900][980,2050]' if rotation=='0' else '[100,700][2274,850]'
    if os.environ.get('BROKEN') and rotation=='1': cta='[100,700][2300,850]'
    print(json.dumps({'children':[{'attributes':{'text':text,'bounds':bounds}} for text,bounds in [('Restore Purchases',footer),('Continue',cta)]]}))
'''
    for executable in ('adb','maestro'):
        path=tmp/executable; path.write_text(fake); path.chmod(0o755)
    env={**os.environ,'PATH':str(tmp)+os.pathsep+os.environ['PATH'],'FAKE_STATE':str(tmp)}
    for scenario in ('pass', 'broken', 'no-reveal', 'no-hide', 'source-only'):
        (tmp/'transient').unlink(missing_ok=True)
        (tmp/'calls').unlink(missing_ok=True)
        out=tmp/scenario
        extra={'broken':{'BROKEN':'1'}, 'no-reveal':{'NO_REVEAL':'1'}, 'no-hide':{'NO_HIDE':'1'}, 'source-only':{'NO_TRANSIENT':'1'}}.get(scenario,{})
        orientations=[] if scenario in ('pass','broken') else ['--orientations','portrait']
        result=subprocess.run(['python3',str(root/'scripts/test-android-paywall-margins.py'),'--device','fake','--output-dir',str(out),*orientations],env={**env,**extra},capture_output=True,text=True)
        passing = scenario in ('pass', 'source-only')
        assert result.returncode==(0 if passing else 1), result.stdout+result.stderr
        suite=ET.parse(out/'junit.xml').getroot()
        assert (int(suite.attrib['failures'])>0)==(not passing), suite.attrib
        assert (tmp/'rotation').read_text()=='2'
        assert (tmp/'fixed').read_text()=='default'
        calls = (tmp/'calls').read_text()
        assert "'enable-exclusive', '--user', '0', '--category', 'com.android.internal.systemui.navbar.gestural'" in calls
        assert "'settings', 'put', 'system', 'accelerometer_rotation', '1'" in calls
        assert "'input', 'swipe'" in calls
        failures=' '.join(e.attrib['message'] for e in suite.findall('testcase/failure'))
        if scenario=='broken':
            assert 'CTA' in failures and '100px' in failures and '126px' in failures, failures
        elif passing:
            count = 2 if scenario == 'pass' else 1
            assert len(list(out.glob('*/hidden-after-reveal/geometry.json')))==count
            assert len(list(out.glob('*/reveal/visible-insets.txt')))==count
            assert len(list(out.glob('*/reveal/hidden-insets.txt')))==count
        else:
            assert 'did not become '+('hidden' if scenario=='no-hide' else 'visible') in failures, failures
            assert list(out.glob('*/reveal/*-timeout-insets.txt'))
        print(scenario+': verified JUnit, geometry/transitions, and settings restoration',flush=True)
