package com.revenuecat.accessibility;

import android.app.Activity;
import android.graphics.Color;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.TextView;

import com.unity3d.player.UnityPlayer;

import java.util.HashMap;
import java.util.Map;

/**
 * Provides a transparent native Android overlay with real TextViews that
 * UIAutomator (and therefore Maestro) can discover in the accessibility tree.
 *
 * Unity UGUI renders everything on a single GLSurfaceView, so automation
 * frameworks that rely on the Android view hierarchy cannot see individual
 * UI elements. This class bridges that gap by placing invisible native
 * views at the same screen coordinates as the Unity UI elements.
 */
public class NativeAccessibilityOverlay {

    private static FrameLayout container;
    private static final Map<String, TextView> elements = new HashMap<>();

    public static void init() {
        final Activity activity = UnityPlayer.currentActivity;
        if (activity == null) return;

        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                if (container != null) return;

                container = new FrameLayout(activity);
                container.setClickable(false);
                container.setFocusable(false);
                container.setFocusableInTouchMode(false);
                container.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_YES);

                FrameLayout.LayoutParams params = new FrameLayout.LayoutParams(
                        ViewGroup.LayoutParams.MATCH_PARENT,
                        ViewGroup.LayoutParams.MATCH_PARENT);
                activity.addContentView(container, params);
            }
        });
    }

    public static void setElement(final String id, final String text,
                                  final int left, final int top,
                                  final int right, final int bottom) {
        final Activity activity = UnityPlayer.currentActivity;
        if (activity == null) return;

        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                if (container == null) return;

                TextView tv = elements.get(id);
                if (tv == null) {
                    tv = new TextView(activity);
                    tv.setTextColor(Color.TRANSPARENT);
                    tv.setBackgroundColor(Color.TRANSPARENT);
                    tv.setClickable(false);
                    tv.setFocusable(false);
                    tv.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_YES);
                    container.addView(tv);
                    elements.put(id, tv);
                }

                tv.setText(text);
                tv.setContentDescription(text);

                FrameLayout.LayoutParams lp = new FrameLayout.LayoutParams(
                        Math.max(right - left, 1),
                        Math.max(bottom - top, 1));
                lp.leftMargin = left;
                lp.topMargin = top;
                tv.setLayoutParams(lp);
                tv.setVisibility(View.VISIBLE);
            }
        });
    }

    public static void removeElement(final String id) {
        final Activity activity = UnityPlayer.currentActivity;
        if (activity == null) return;

        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                if (container == null) return;
                TextView tv = elements.remove(id);
                if (tv != null) container.removeView(tv);
            }
        });
    }

    public static void clear() {
        final Activity activity = UnityPlayer.currentActivity;
        if (activity == null) return;

        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                if (container == null) return;
                container.removeAllViews();
                elements.clear();
            }
        });
    }
}
