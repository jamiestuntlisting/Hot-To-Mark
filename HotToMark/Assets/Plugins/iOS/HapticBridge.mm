// HapticBridge.mm
// Native iOS haptic feedback bridge for Unity.
// Provides Core Haptics access via C functions called from C#.

#import <UIKit/UIKit.h>

extern "C" {
    void _TriggerImpactHaptic(int style) {
        UIImpactFeedbackStyle feedbackStyle;
        switch (style) {
            case 0: feedbackStyle = UIImpactFeedbackStyleLight; break;
            case 1: feedbackStyle = UIImpactFeedbackStyleMedium; break;
            case 2: feedbackStyle = UIImpactFeedbackStyleHeavy; break;
            default: feedbackStyle = UIImpactFeedbackStyleMedium; break;
        }
        UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc]
            initWithStyle:feedbackStyle];
        [generator prepare];
        [generator impactOccurred];
    }

    void _TriggerNotificationHaptic(int type) {
        UINotificationFeedbackType feedbackType;
        switch (type) {
            case 0: feedbackType = UINotificationFeedbackTypeSuccess; break;
            case 1: feedbackType = UINotificationFeedbackTypeWarning; break;
            case 2: feedbackType = UINotificationFeedbackTypeError; break;
            default: feedbackType = UINotificationFeedbackTypeSuccess; break;
        }
        UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
        [generator prepare];
        [generator notificationOccurred:feedbackType];
    }

    void _TriggerSelectionHaptic() {
        UISelectionFeedbackGenerator *generator = [[UISelectionFeedbackGenerator alloc] init];
        [generator prepare];
        [generator selectionChanged];
    }
}
