﻿#pragma checksum "C:\git\HuntMapOverlay\App.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "B2AA79A0A5B80DEF7C421ED6AE5BBCACCE2F7F809B682F2D3C7BE808661A9AD0"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace HuntMapOverlay
{
#if !DISABLE_XAML_GENERATED_MAIN
    /// <summary>
    /// Program class
    /// </summary>
    public static class Program
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 0.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        static void Main(string[] args)
        {
            global::Windows.UI.Xaml.Application.Start((p) => new App());
        }
    }
#endif

    partial class App : global::Windows.UI.Xaml.Application
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 0.0.0.0")]
        private bool _contentLoaded;
        /// <summary>
        /// InitializeComponent()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 0.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent()
        {
            if (_contentLoaded)
                return;

            _contentLoaded = true;
#if DEBUG && !DISABLE_XAML_GENERATED_BINDING_DEBUG_OUTPUT
            DebugSettings.BindingFailed += (sender, args) =>
            {
                global::System.Diagnostics.Debug.WriteLine(args.Message);
            };
#endif
#if DEBUG && !DISABLE_XAML_GENERATED_BREAK_ON_UNHANDLED_EXCEPTION
            UnhandledException += (sender, e) =>
            {
                if (global::System.Diagnostics.Debugger.IsAttached) global::System.Diagnostics.Debugger.Break();
            };
#endif
        }
    }
}

