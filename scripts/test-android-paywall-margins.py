#!/usr/bin/env python3
"""Run Maestro paywall gutter assertions on an installed Android test app."""
import argparse
import json
import re
from pathlib import Path
import subprocess
import sys
import time
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
FLOWS = ROOT / 'e2e-tests/maestro/layout_tests'
NAV_OVERLAY = 'com.android.internal.systemui.navbar.threebutton'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--device', required=True, help='adb serial / Maestro device ID')
    parser.add_argument('--app-id', default='com.revenuecat.automatedsdktests')
    parser.add_argument('--output-dir', type=Path, default=ROOT / 'fastlane/test_output/android-paywall-margins')
    parser.add_argument('--already-open', action='store_true', help='Use the currently open paywall instead of launching the Unity test app')
    parser.add_argument('--orientations', nargs='+', choices=('portrait', 'landscape'),
                        default=['portrait', 'landscape'], help='Orientations to check (default: both)')
    parser.add_argument('--footer-text', default='Restore Purchases')
    parser.add_argument('--cta-text', default='Continue')
    args = parser.parse_args()
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=True)
    suite = ET.Element('testsuite', name='Android paywall navigation margins')
    log = (output / 'commands.log').open('w')

    def run(command, destination=None, check=True):
        print(' '.join(map(str, command)), flush=True)
        result = subprocess.run(list(map(str, command)), text=True, capture_output=True, timeout=120)
        log.write(f'$ {" ".join(map(str, command))}\n{result.stdout}\n{result.stderr}\n')
        log.flush()
        if destination:
            destination.write_text(result.stdout)
        if check and result.returncode:
            raise RuntimeError(f'Command exited {result.returncode}: {command}\n{result.stdout}\n{result.stderr}')
        return result

    def adb(*command):
        return run(['adb', '-s', args.device, 'shell', *command]).stdout.strip()

    def maestro(flow, directory, **variables):
        directory.mkdir(parents=True, exist_ok=True)
        command = ['maestro', '--device', args.device, '--no-ansi', 'test', '--no-reinstall-driver', '--format', 'JUNIT',
                   '--output', directory / 'maestro.xml', '--test-output-dir', directory,
                   '--debug-output', directory / 'debug', '-e', f'APP_ID={args.app_id}',
                   '-e', f'FOOTER_TEXT={args.footer_text}', '-e', f'CTA_TEXT={args.cta_text}']
        for key, value in variables.items():
            command.extend(['-e', f'{key}={value}'])
        run([*command, FLOWS / flow])

    def case(name, action):
        started = time.monotonic()
        element = ET.SubElement(suite, 'testcase', name=name, classname='AndroidPaywallMargins')
        try:
            action()
            return True
        except (OSError, RuntimeError, ValueError, subprocess.TimeoutExpired) as error:
            ET.SubElement(element, 'failure', message=str(error)).text = str(error)
            print(f'FAIL {name}: {error}', file=sys.stderr, flush=True)
            return False
        finally:
            element.set('time', f'{time.monotonic() - started:.3f}')

    def capture(orientation, stage):
        directory = output / orientation / stage
        maestro('capture_paywall.yaml', directory, SCREENSHOT_NAME=str(directory / 'paywall'))
        run(['maestro', '--device', args.device, '--no-ansi', 'hierarchy', '--no-reinstall-driver'], directory / 'hierarchy.json')
        run(['adb', '-s', args.device, 'shell', 'dumpsys', 'window', 'displays'], directory / 'insets.txt')
        result = run([sys.executable, ROOT / 'scripts/assert-android-paywall-margins.py',
                      '--hierarchy', directory / 'hierarchy.json', '--insets', directory / 'insets.txt',
                      '--footer-text', args.footer_text, '--cta-text', args.cta_text,
                      '--report', directory / 'geometry.json'], check=False)
        if result.returncode:
            raise RuntimeError(result.stdout or result.stderr)
        report = json.loads(result.stdout)
        width = report['display'][2] - report['display'][0]
        height = report['display'][3] - report['display'][1]
        if (width < height) != (orientation == 'portrait'):
            raise RuntimeError(f'Expected {orientation}, captured display {width}x{height}')
        sources = [line for line in (directory / 'insets.txt').read_text().splitlines()
                   if re.search(r'\bInsetsSource\b', line) and re.search(r'\b(?:type|mType)=navigationBars\b', line)]
        if not sources or any(not re.search(r'\b(?:visible|mVisible)=false\b', line) for line in sources):
            raise RuntimeError('Capture requires hidden navigation controls (navigationBars visible=false)')

    def wait_for_rotation(orientation):
        deadline = time.monotonic() + 10
        while time.monotonic() < deadline:
            window = adb('dumpsys', 'window', 'displays')
            frames = {tuple(map(int, rect)) for rect in re.findall(
                r'mDisplayFrame=Rect\(\s*(-?\d+),\s*(-?\d+)\s*-\s*(-?\d+),\s*(-?\d+)\)', window)}
            if len(frames) == 1:
                left, top, right, bottom = next(iter(frames))
                if (right - left < bottom - top) == (orientation == 'portrait'):
                    return
            time.sleep(0.25)
        raise RuntimeError(f'Display did not rotate to {orientation} within 10 seconds')

    saved_settings = {}
    saved_overlays = None
    saved_fixed_rotation = None
    saved_wm_rotation = None
    try:
        for setting in ('accelerometer_rotation', 'user_rotation'):
            saved_settings[setting] = adb('settings', 'get', 'system', setting)
        fixed = adb('wm', 'fixed-to-user-rotation')
        if fixed not in ('default', 'disabled', 'enabled', 'enabled_if_no_auto_rotation'):
            raise RuntimeError(f'Unexpected wm fixed-to-user-rotation setting: {fixed!r}')
        saved_fixed_rotation = fixed
        rotation_mode = adb('wm', 'user-rotation')
        if not re.fullmatch(r'free|lock [0-3]', rotation_mode):
            raise RuntimeError(f'Unexpected wm user-rotation setting: {rotation_mode!r}')
        saved_wm_rotation = rotation_mode.split()
        overlays = adb('cmd', 'overlay', 'list', '--user', '0')
        saved_overlays = [line.split()[-1] for line in overlays.splitlines()
                          if line.strip().startswith('[x]') and 'com.android.internal.systemui.navbar.' in line]
        if NAV_OVERLAY not in overlays:
            raise RuntimeError(f'Three-button navigation overlay unavailable: {NAV_OVERLAY}')
        adb('cmd', 'overlay', 'enable-exclusive', '--user', '0', '--category', NAV_OVERLAY)
        adb('settings', 'put', 'system', 'accelerometer_rotation', '0')
        adb('wm', 'fixed-to-user-rotation', 'enabled')
        for orientation, rotation in (('portrait', '0'), ('landscape', '1')):
            if orientation not in args.orientations:
                continue
            def prepare():
                adb('settings', 'put', 'system', 'user_rotation', rotation)
                adb('wm', 'user-rotation', 'lock', rotation)
                wait_for_rotation(orientation)
                if not args.already_open:
                    maestro('open_default_paywall.yaml', output / orientation / 'open')
            if not case(f'{orientation}.open', prepare):
                continue
            case(f'{orientation}.initial-hidden', lambda: capture(orientation, 'initial-hidden'))
            # The flow waits for Back to appear and then disappear, proving the
            # second capture follows an actual transient-navigation cycle.
            def reveal_and_capture():
                report = json.loads((output / orientation / 'initial-hidden/geometry.json').read_text())
                if 'navigation_edge' not in report:
                    raise RuntimeError('Initial capture has no valid navigation edge')
                edge = report['navigation_edge']
                maestro('reveal_navigation.yaml', output / orientation / 'reveal', NAVIGATION_EDGE=edge)
                capture(orientation, 'hidden-after-reveal')
            case(f'{orientation}.hidden-after-reveal', reveal_and_capture)
    except (OSError, RuntimeError, ValueError, subprocess.TimeoutExpired) as error:
        case('setup', lambda: (_ for _ in ()).throw(RuntimeError(str(error))))
    finally:
        def restore():
            for setting, value in saved_settings.items():
                if value == 'null':
                    adb('settings', 'delete', 'system', setting)
                else:
                    adb('settings', 'put', 'system', setting, value)
            if saved_wm_rotation is not None:
                adb('wm', 'user-rotation', *saved_wm_rotation)
            if saved_fixed_rotation is not None:
                adb('wm', 'fixed-to-user-rotation', saved_fixed_rotation)
            if saved_overlays is not None:
                adb('cmd', 'overlay', 'disable', '--user', '0', NAV_OVERLAY)
                for overlay in saved_overlays:
                    adb('cmd', 'overlay', 'enable-exclusive', '--user', '0', '--category', overlay)
        case('restore-device-settings', restore)
        failures = len(suite.findall('testcase/failure'))
        suite.set('tests', str(len(suite.findall('testcase'))))
        suite.set('failures', str(failures))
        ET.ElementTree(suite).write(output / 'junit.xml', encoding='utf-8', xml_declaration=True)
        log.close()
    return 1 if failures else 0


if __name__ == '__main__':
    sys.exit(main())
