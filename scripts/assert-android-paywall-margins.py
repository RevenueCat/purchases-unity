#!/usr/bin/env python3
"""Check rendered Maestro paywall bounds against Android's physical nav gutter.

Navigation source visibility is deliberately ignored: hiding system controls must
not let paywall content consume the space reserved for those controls.
"""
import argparse
import json
from pathlib import Path
import re
import sys

BOUNDS = r'\[\s*(-?\d+),\s*(-?\d+)\]\[\s*(-?\d+),\s*(-?\d+)\]'
DISPLAY = r'mDisplayFrame=Rect\(\s*(-?\d+),\s*(-?\d+)\s*-\s*(-?\d+),\s*(-?\d+)\)'


def valid_rect(rect, label, display=None):
    left, top, right, bottom = rect
    if left >= right or top >= bottom:
        raise ValueError(f'{label}: empty or inverted bounds {rect}')
    if display and not contains(display, rect):
        raise ValueError(f'{label}: bounds {rect} outside display {display}')
    return rect


def contains(outer, inner):
    return outer[0] <= inner[0] and outer[1] <= inner[1] and outer[2] >= inner[2] and outer[3] >= inner[3]


def display_and_navigation(window):
    displays = {tuple(map(int, match)) for match in re.findall(DISPLAY, window)}
    if len(displays) != 1:
        raise ValueError('Missing or ambiguous mDisplayFrame; capture current display after rotation')
    display = valid_rect(next(iter(displays)), 'display')
    frames = set()
    for line in window.splitlines():
        if re.search(r'\bInsetsSource\b', line) and re.search(r'\b(?:type|mType)=navigationBars\b', line):
            match = re.search(r'\b(?:frame|mFrame)=' + BOUNDS, line)
            if not match:
                raise ValueError('navigationBars source is missing a valid frame')
            frames.add(valid_rect(tuple(map(int, match.groups())), 'navigationBars', display))
    if len(frames) != 1:
        raise ValueError('Missing or ambiguous navigationBars frame (visibility does not matter)')
    nav = next(iter(frames))
    left, top, right, bottom = display
    edges = []
    if nav[0] == left and nav[2] == right and nav[3] == bottom and nav[1] > top:
        edges.append('bottom')
    if nav[0] == left and nav[1] == top and nav[3] == bottom and nav[2] < right:
        edges.append('left')
    if nav[2] == right and nav[1] == top and nav[3] == bottom and nav[0] > left:
        edges.append('right')
    if len(edges) != 1:
        raise ValueError(f'navigationBars frame {nav} is not an unambiguous full display edge')
    return display, nav, edges[0]


def labelled_bounds(tree, text, label, display):
    matches = set()

    def rendered_bounds(attributes):
        match = re.fullmatch(BOUNDS, str(attributes.get('bounds', '')))
        if not match:
            raise ValueError(f'{label} ({text}): missing or malformed rendered bounds')
        return valid_rect(tuple(map(int, match.groups())), label, display)

    def visit(node, clickable_parent=None):
        if not isinstance(node, dict):
            raise ValueError('Malformed Maestro hierarchy node')
        attributes = node.get('attributes', {})
        if not isinstance(attributes, dict):
            raise ValueError('Malformed Maestro attributes')
        if str(attributes.get('clickable', node.get('clickable', False))).casefold() == 'true':
            clickable_parent = attributes
        if any(str(attributes.get(key, '')).casefold() == text.casefold() for key in ('text', 'accessibilityText')):
            label_bounds = rendered_bounds(attributes)
            bounds = rendered_bounds(clickable_parent) if label == 'CTA' and clickable_parent is not None else label_bounds
            if not contains(bounds, label_bounds):
                raise ValueError(f'{label} ({text}): clickable bounds do not enclose rendered label')
            matches.add(bounds)
        children = node.get('children', [])
        if not isinstance(children, list):
            raise ValueError('Malformed Maestro children')
        for child in children:
            visit(child, clickable_parent)

    visit(tree)
    if not matches:
        raise ValueError(f'Missing {label} with exact text/accessibilityText {text!r}')
    # Measure the CTA's clickable button, but the footer's visible text: Compose
    # may enlarge its invisible accessibility hit area into the reserved gutter.
    # Use encompassing bounds when duplicate labelled nodes describe one control.
    encompassing = [rect for rect in matches if all(contains(rect, other) for other in matches)]
    if len(encompassing) != 1:
        raise ValueError(f'Ambiguous {label} ({text}): disjoint or overlapping labelled bounds')
    return encompassing[0]


def check(tree, window, footer_text, cta_text):
    report = {'passed': False, 'errors': [], 'measurements': {}}
    try:
        display, nav, edge = display_and_navigation(window)
        report.update(display=list(display), navigation=list(nav), navigation_edge=edge)
        for label, text in (('footer', footer_text), ('CTA', cta_text)):
            try:
                bounds = labelled_bounds(tree, text, label, display)
                if edge == 'bottom':
                    gap, required = display[3] - bounds[3], display[3] - nav[1]
                elif edge == 'right':
                    gap, required = display[2] - bounds[2], display[2] - nav[0]
                else:
                    gap, required = bounds[0] - display[0], nav[2] - display[0]
                report['measurements'][label.lower()] = {
                    'text': text, 'bounds': list(bounds), 'gap': gap,
                    'required_gutter': required, 'passed': gap >= required,
                }
                if gap < required:
                    report['errors'].append(f'{label} ({text}): {edge} gap {gap}px is below required navigation gutter {required}px')
            except ValueError as error:
                report['errors'].append(str(error))
    except ValueError as error:
        report['errors'].append(str(error))
    report['passed'] = not report['errors']
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--hierarchy', required=True, type=Path)
    parser.add_argument('--insets', required=True, type=Path)
    parser.add_argument('--footer-text', default='Restore Purchases')
    parser.add_argument('--cta-text', default='Continue')
    parser.add_argument('--report', type=Path)
    args = parser.parse_args()
    try:
        report = check(json.loads(args.hierarchy.read_text()), args.insets.read_text(), args.footer_text, args.cta_text)
    except (OSError, ValueError) as error:
        report = {'passed': False, 'errors': [str(error)], 'measurements': {}}
    output = json.dumps(report, indent=2) + '\n'
    if args.report:
        args.report.write_text(output)
    print(output, end='')
    return 0 if report['passed'] else 1


if __name__ == '__main__':
    sys.exit(main())
