using System;
using System.IO;
using System.Media;
using System.Windows;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace DocumentSearch.Services;

public static class NotificationService
{
    public static void ShowNotification(string title, string message)
    {
        // 1. Sesli Uyarı (Sound)
        PlayCompletionSound();

        // 2. Masaüstü Bildirimi (Windows Toast Notification)
        ShowWindowsToast(title, message);
    }

    private static void PlayCompletionSound()
    {
        try
        {
            // SystemSounds.Exclamation plays a clean, pleasant notification sound
            SystemSounds.Exclamation.Play();
        }
        catch
        {
            try
            {
                SystemSounds.Asterisk.Play();
            }
            catch
            {
                // Fallback
            }
        }
    }

    private static void ShowWindowsToast(string title, string message)
    {
        try
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var stringElements = toastXml.GetElementsByTagName("text");
            if (stringElements.Count >= 2)
            {
                stringElements[0].AppendChild(toastXml.CreateTextNode(title));
                stringElements[1].AppendChild(toastXml.CreateTextNode(message));

                var toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier("E-Student").Show(toast);
            }
        }
        catch
        {
            // Unpackaged Win32 app toast fallback or notification silenced
        }
    }
}
