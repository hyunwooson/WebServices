using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace F1Widget.Droid
{
    [BroadcastReceiver(Label = "F1Widget")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [IntentFilter(new string[] { "com.hyunwoo.f1widget.ACTION_WIDGET_TURNON" })]
    [IntentFilter(new string[] { "com.hyunwoo.f1widget.ACTION_WIDGET_TURNOFF" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/f1_widget_provider")]

    class f1_widget_class : AppWidgetProvider
    {
        public static String ACTION_WIDGET_TURNFON = "button 1 click";
        public static String ACTION_WIDGET_TURNFOFF = "button 2 click";
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            //update widget layout
            //run when create widget or meet update interval
            base.OnUpdate(context, appWidgetManager, appWidgetIds);
            var me = new ComponentName(context, Java.Lang.Class.FromType(typeof(f1_widget_class)).Name);
            appWidgetManager.UpdateAppWidget(me, BuildRemoteViews(context, appWidgetIds));

        }

        private RemoteViews BuildRemoteViews(Context context, int[] appWidgetIds)
        {
            //build widget layout
            var widgetView = new RemoteViews(context.PackageName, Resource.Layout.f1_widget);

            //change text of element on widget
            SetTextViewText(widgetView);

            //handle click event of button on widget
            RegisterClicks(context, appWidgetIds, widgetView);

            return widgetView;
        }

        private void RegisterClicks(Context context, int[] appWidgetIds, RemoteViews widgetView)
        {
            var intent = new Intent(context, type: typeof(f1_widget_class));
            intent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
            intent.PutExtra(name: AppWidgetManager.ExtraAppwidgetId, appWidgetIds);

            //button turn off
            widgetView.SetOnClickPendingIntent(Resource.Id.button2, GetpendingSelfIntent(context, ACTION_WIDGET_TURNFOFF));

            //button turn on
            widgetView.SetOnClickPendingIntent(Resource.Id.button1, GetpendingSelfIntent(context, ACTION_WIDGET_TURNFON));
        }

        private PendingIntent GetpendingSelfIntent(Context context, string action)
        {
            var intent = new Intent(context, type: typeof(f1_widget_class));
            intent.SetAction(action);
            return PendingIntent.GetBroadcast(context, requestCode: 0, intent, flags: 0);
        }

        private void SetTextViewText(RemoteViews widgetView)
        {
            widgetView.SetTextViewText(Resource.Id.textView2, text: "HelloWorld");
        }

        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);

            //check if the click is from the "ACTION_WIDGET_TURNOFF" or "ACTION_WIDGET_TURNON" button
            if (ACTION_WIDGET_TURNFOFF.Equals(intent.Action))
            {
                Toast.MakeText(context, text: "show me the button1", duration: ToastLength.Short).Show();
            }
            if (ACTION_WIDGET_TURNFON.Equals(intent.Action))
            {
                Toast.MakeText(context, text: "hello the button2", duration: ToastLength.Short).Show();
            }
        }
    }
}