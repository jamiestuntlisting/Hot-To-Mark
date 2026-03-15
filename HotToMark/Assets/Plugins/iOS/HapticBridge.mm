// HapticBridge.mm
// Native iOS haptic feedback bridge for Unity.
// Provides Core Haptics access via C functions called from C#.
// Generators are cached for low-latency repeated triggers.

#import <UIKit/UIKit.h>

// Cached generators for performance — avoids alloc+prepare on every call
static UIImpactFeedbackGenerator *_lightGenerator = nil;
static UIImpactFeedbackGenerator *_mediumGenerator = nil;
static UIImpactFeedbackGenerator *_heavyGenerator = nil;
static UINotificationFeedbackGenerator *_notificationGenerator = nil;
static UISelectionFeedbackGenerator *_selectionGenerator = nil;

static void _EnsureGenerators() {
    if (_lightGenerator == nil) {
        _lightGenerator = [[UIImpactFeedbackGenerator alloc]
            initWithStyle:UIImpactFeedbackStyleLight];
        [_lightGenerator prepare];
    }
    if (_mediumGenerator == nil) {
        _mediumGenerator = [[UIImpactFeedbackGenerator alloc]
            initWithStyle:UIImpactFeedbackStyleMedium];
        [_mediumGenerator prepare];
    }
    if (_heavyGenerator == nil) {
        _heavyGenerator = [[UIImpactFeedbackGenerator alloc]
            initWithStyle:UIImpactFeedbackStyleHeavy];
        [_heavyGenerator prepare];
    }
    if (_notificationGenerator == nil) {
        _notificationGenerator = [[UINotificationFeedbackGenerator alloc] init];
        [_notificationGenerator prepare];
    }
    if (_selectionGenerator == nil) {
        _selectionGenerator = [[UISelectionFeedbackGenerator alloc] init];
        [_selectionGenerator prepare];
    }
}

extern "C" {
    void _TriggerImpactHaptic(int style) {
        _EnsureGenerators();
        switch (style) {
            case 0:
                [_lightGenerator impactOccurred];
                [_lightGenerator prepare];  // re-prepare for next trigger
                break;
            case 1:
                [_mediumGenerator impactOccurred];
                [_mediumGenerator prepare];
                break;
            case 2:
                [_heavyGenerator impactOccurred];
                [_heavyGenerator prepare];
                break;
            default:
                [_mediumGenerator impactOccurred];
                [_mediumGenerator prepare];
                break;
        }
    }

    void _TriggerNotificationHaptic(int type) {
        _EnsureGenerators();
        UINotificationFeedbackType feedbackType;
        switch (type) {
            case 0: feedbackType = UINotificationFeedbackTypeSuccess; break;
            case 1: feedbackType = UINotificationFeedbackTypeWarning; break;
            case 2: feedbackType = UINotificationFeedbackTypeError; break;
            default: feedbackType = UINotificationFeedbackTypeSuccess; break;
        }
        [_notificationGenerator notificationOccurred:feedbackType];
        [_notificationGenerator prepare];
    }

    void _TriggerSelectionHaptic() {
        _EnsureGenerators();
        [_selectionGenerator selectionChanged];
        [_selectionGenerator prepare];
    }
}
