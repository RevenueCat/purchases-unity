#!/usr/bin/env python3
"""Behavioral tests using hand-measured Maestro and dumpsys fixtures."""
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

SCRIPT = Path(__file__).with_name('assert-android-paywall-margins.py')


def node(text, bounds, **attributes):
    return {'attributes': {'text': text, 'bounds': bounds, **attributes}, 'children': []}


def hierarchy(footer='[100,2100][980,2274]', cta='[100,1900][980,2050]'):
    return {'attributes': {}, 'children': [node('Restore Purchases', footer), node('Continue', cta)]}


def insets(display='Rect(0, 0 - 1080, 2400)', nav='[0,2274][1080,2400]', visible='false'):
    return f'mDisplayFrame={display}\n InsetsSource id=3 type=navigationBars frame={nav} visible={visible} flags=\n'


class MarginTests(unittest.TestCase):
    def run_check(self, tree=None, window=None, *extra):
        with tempfile.TemporaryDirectory() as directory:
            directory = Path(directory)
            (directory / 'hierarchy.json').write_text(json.dumps(hierarchy() if tree is None else tree))
            (directory / 'insets.txt').write_text(insets() if window is None else window)
            result = subprocess.run([sys.executable, str(SCRIPT), '--hierarchy', str(directory / 'hierarchy.json'),
                                     '--insets', str(directory / 'insets.txt'), '--report', str(directory / 'report.json'), *extra],
                                    capture_output=True, text=True)
            self.assertTrue((directory / 'report.json').exists(), result.stderr)
            report = json.loads((directory / 'report.json').read_text())
            self.assertEqual(json.loads(result.stdout), report)
            self.assertEqual(result.returncode == 0, report['passed'])
            return report

    def test_exact_fit_preserves_hidden_bottom_navigation_gutter(self):
        report = self.run_check()
        self.assertTrue(report['passed'], report)
        self.assertEqual(report['navigation_edge'], 'bottom')
        self.assertEqual(report['measurements']['footer']['gap'], 126)
        self.assertEqual(report['measurements']['footer']['required_gutter'], 126)

    def test_footer_below_safe_edge_fails_with_measured_gap(self):
        report = self.run_check(hierarchy(footer='[100,2200][980,2300]'))
        self.assertFalse(report['passed'])
        self.assertEqual(report['measurements']['footer']['gap'], 100)
        self.assertIn('footer', ' '.join(report['errors']))
        self.assertIn('126', ' '.join(report['errors']))

    def test_visible_and_initially_hidden_controls_have_identical_requirement(self):
        hidden = self.run_check(hierarchy(footer='[100,2200][980,2390]'))
        shown = self.run_check(hierarchy(footer='[100,2200][980,2390]'), insets(visible='true'))
        self.assertFalse(hidden['passed'])
        self.assertEqual(hidden['measurements'], shown['measurements'])

    def test_rotated_right_navigation_rejects_cta_overrun(self):
        tree = hierarchy('[100,900][2200,1000]', '[100,700][2300,850]')
        report = self.run_check(tree, insets('Rect(0, 0 - 2400, 1080)', '[2274,0][2400,1080]'))
        self.assertFalse(report['passed'])
        self.assertEqual(report['navigation_edge'], 'right')
        self.assertEqual(report['measurements']['cta']['gap'], 100)
        self.assertIn('CTA', ' '.join(report['errors']))

    def test_source_controls_without_frames_do_not_mask_real_source(self):
        controls = ('InsetsSourceControl: {6c680001 mType=navigationBars initiallyVisible '
                    'mSurfacePosition=Point(0, 2274) mInsetsHint=Insets{left=0, top=0, right=0, bottom=126}}\n'
                    'mControl=InsetsSourceControl mId=6c680001 mType=navigationBars initiallyVisible\n')
        for source in (
            'InsetsSource id=6c680001 type=navigationBars frame=[0,2274][1080,2400] visible=false flags= insetsRoundedCornerFrame=false',
            'InsetsSource: {6c680001 mType=navigationBars mFrame=[0,2274][1080,2400] mVisible=false}',
        ):
            with self.subTest(source=source):
                window = 'mDisplayFrame=Rect(0, 0 - 1080, 2400)\n' + controls + source + '\n'
                self.assertTrue(self.run_check(window=window)['passed'])
        self.assertFalse(self.run_check(window='mDisplayFrame=Rect(0, 0 - 1080, 2400)\n' + controls)['passed'])

    def test_left_navigation_exact_fit_and_api36_fields(self):
        tree = hierarchy('[126,900][2200,1000]', '[126,700][2300,850]')
        window = 'mDisplayFrame=Rect(0, 0 - 2400, 1080)\nInsetsSource mType=navigationBars mFrame=[0,0][126,1080] mVisible=false\n'
        report = self.run_check(tree, window)
        self.assertTrue(report['passed'], report)
        self.assertEqual(report['navigation_edge'], 'left')
        self.assertEqual(report['measurements']['cta']['gap'], 126)
        tree['children'][1]['attributes']['bounds'] = '[100,700][2300,850]'
        self.assertFalse(self.run_check(tree, window)['passed'])

    def test_missing_and_unsafe_geometry_fail_closed(self):
        cases = [
            ({'attributes': {}, 'children': []}, insets()),
            (hierarchy(), 'mDisplayFrame=Rect(0, 0 - 1080, 2400)'),
            (hierarchy(), 'InsetsSource type=navigationBars frame=[0,2274][1080,2400]'),
            (hierarchy(), insets(nav='[0,0][0,0]')),
            (hierarchy(), insets(nav='[0,2250][1000,2400]')),
            (hierarchy(), insets(nav='[0,2274][1080,2401]')),
            (hierarchy(footer='nonsense'), insets()),
            (hierarchy(footer='[100,2200][1100,2300]'), insets()),
            (hierarchy(footer='[100,2200][100,2300]'), insets()),
            (hierarchy(), insets() + 'InsetsSource type=navigationBars frame=[0,0][126,2400]\n'),
            (hierarchy(), insets() + 'mDisplayFrame=Rect(0, 0 - 2400, 1080)\n'),
        ]
        for tree, window in cases:
            with self.subTest(tree=tree, window=window):
                report = self.run_check(tree, window)
                self.assertFalse(report['passed'])
                self.assertTrue(report['errors'])

    def test_case_insensitive_accessibility_and_configurable_labels(self):
        tree = {'children': [node('', '[100,2100][980,2274]', accessibilityText='RESTORE'),
                             node('BUY', '[100,1900][980,2050]')]}
        self.assertTrue(self.run_check(tree, None, '--footer-text', 'Restore', '--cta-text', 'buy')['passed'])

    def test_duplicate_and_nested_matches_use_largest_rendered_rectangle(self):
        tree = hierarchy(footer='[100,2200][980,2300]')
        tree['children'] += [node('Restore Purchases', '[200,2205][800,2250]'),
                             node('Restore Purchases', '[100,2200][980,2300]', clickable='true')]
        report = self.run_check(tree)
        self.assertFalse(report['passed'])
        self.assertEqual(report['measurements']['footer']['bounds'], [100, 2200, 980, 2300])

    def test_unlabelled_clickable_cta_parent_reserves_its_full_width(self):
        window = insets('Rect(0, 0 - 2400, 1080)', '[2274,0][2400,1080]')
        for parent_bounds, passed in (('[100,700][2300,850]', False), ('[100,700][2274,850]', True)):
            with self.subTest(parent_bounds=parent_bounds):
                tree = hierarchy('[100,900][2200,1000]')
                tree['children'][1] = node('', parent_bounds, clickable='true')
                tree['children'][1]['children'] = [node('Continue', '[1000,740][1300,790]')]
                report = self.run_check(tree, window)
                self.assertEqual(report['passed'], passed, report)
                self.assertEqual(report['measurements']['cta']['bounds'][2], 2300 if not passed else 2274)

    def test_footer_visual_text_excludes_compose_accessibility_hit_slop(self):
        tree = hierarchy()
        parent = node('', '[387,2191][693,2317]', clickable='true')
        parent['children'] = [node('Restore Purchases', '[387,2234][693,2274]')]
        tree['children'][0] = parent
        report = self.run_check(tree)
        self.assertTrue(report['passed'], report)
        self.assertEqual(report['measurements']['footer']['bounds'], [387, 2234, 693, 2274])
        self.assertEqual(report['measurements']['footer']['gap'], 126)

    def test_nearest_clickable_parent_wins_over_unrelated_outer_container(self):
        tree = hierarchy()
        label = node('Continue', '[200,2200][800,2250]')
        button = node('', '[100,2100][980,2274]', clickable='true')
        button['children'] = [label]
        outer = node('', '[0,0][1080,2400]', clickable='true')
        outer['children'] = [button]
        tree['children'][1] = outer
        report = self.run_check(tree)
        self.assertTrue(report['passed'], report)
        self.assertEqual(report['measurements']['cta']['bounds'], [100, 2100, 980, 2274])
        label['attributes']['clickable'] = 'true'
        report = self.run_check(tree)
        self.assertEqual(report['measurements']['cta']['bounds'], [200, 2200, 800, 2250])

    def test_malformed_or_non_enclosing_clickable_parent_fails_closed(self):
        for bounds in ('missing', '[0,0][1080,2401]', '[100,1900][980,1950]'):
            with self.subTest(bounds=bounds):
                tree = hierarchy()
                parent = node('', bounds, clickable='true')
                parent['children'] = [tree['children'][1]]
                tree['children'][1] = parent
                self.assertFalse(self.run_check(tree)['passed'])

    def test_disjoint_matches_are_ambiguous_even_when_each_is_safe(self):
        tree = hierarchy()
        tree['children'].append(node('Continue', '[100,1500][980,1650]'))
        self.assertFalse(self.run_check(tree)['passed'])

    def test_missing_cta_and_partial_text_do_not_match(self):
        tree = hierarchy()
        tree['children'][1]['attributes']['text'] = 'Continue now'
        self.assertFalse(self.run_check(tree)['passed'])


if __name__ == '__main__':
    unittest.main()
