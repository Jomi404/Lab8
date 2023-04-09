using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using DiagramClassEditor.Models;
using DiagramClassEditor.Views;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;
using System.Xml.Linq;

namespace DiagramClassEditor.ViewModels {
    public class Log {
        static readonly List<string> logs = new();

        public static MainWindowViewModel? Mwvm { private get; set; }
        public static void Write(string message, bool without_update = false) {
            if (!without_update) {
                foreach (var mess in message.Split('\n')) logs.Add(mess);
                while (logs.Count > 50) logs.RemoveAt(0);

                if (Mwvm != null) Mwvm.Logg = string.Join('\n', logs);
            }
        }
    }

    public class MainWindowViewModel: ViewModelBase {
        private string log = "";
        private readonly Canvas canv;
        private readonly Mapper map;
        private AddShape? menu;

        public string Logg { get => log; set => this.RaiseAndSetIfChanged(ref log, value); }

        public MainWindowViewModel(Window mw) {
            Log.Mwvm = this;
            map = new Mapper();

            AddFirstAttr = ReactiveCommand.Create<Unit, Unit>(_ => { FuncAddFirstAttr(); return new Unit(); });
            AddFirstMethod = ReactiveCommand.Create<Unit, Unit>(_ => { FuncAddFirstMethod(); return new Unit(); });

            Apply = ReactiveCommand.Create<Unit, Unit>(_ => { FuncApply(); return new Unit(); });
            Close = ReactiveCommand.Create<Unit, Unit>(_ => { FuncClose(); return new Unit(); });
            Clear = ReactiveCommand.Create<Unit, Unit>(_ => { FuncClear(); return new Unit(); });

            canv = mw.Find<Canvas>("canvas") ?? new Canvas();
            var panel = (Panel?) canv.Parent;
            if (panel == null) return;

            Log.Write("May be..");
            panel.PointerPressed += (object? sender, PointerPressedEventArgs e) => {
                Log.Write("PointerPressed: " + (e.Source == null ? "null" : e.Source.GetType().Name) + " pos: " + e.GetCurrentPoint(canv).Position);               
            };
            panel.PointerMoved += (object? sender, PointerEventArgs e) => {
                Log.Write("PointerMoved: " + (e.Source == null ? "null" : e.Source.GetType().Name) + " pos: " + e.GetCurrentPoint(canv).Position);
            };
            panel.PointerReleased += (object? sender, PointerReleasedEventArgs e) => {
                Log.Write("PointerReleased: " + (e.Source == null ? "null" : e.Source.GetType().Name) + " pos: " + e.GetCurrentPoint(canv).Position);
                menu = new AddShape() { DataContext = this };
                menu.ShowDialog(mw);
            };
        }
        
        string name = "May be..";
        int stereo = 0; 
        int access = 0; 

        public string Name { get => name; set => this.RaiseAndSetIfChanged(ref name, value); }

        public bool Stereo_1 { get => stereo == 0; set => stereo = value ? 0 : -1; }
        public bool Stereo_2 { get => stereo == 1; set => stereo = value ? 1 : -1; }
        public bool Stereo_3 { get => stereo == 2; set => stereo = value ? 2 : -1; }

        public bool Access_1 { get => access == 0; set => access = value ? 0 : -1; }
        public bool Access_2 { get => access == 1; set => access = value ? 1 : -1; }
        public bool Access_3 { get => access == 2; set => access = value ? 2 : -1; }
        public bool Access_4 { get => access == 3; set => access = value ? 3 : -1; }

        readonly ObservableCollection<AttributeItem> attributes = new();
        public ObservableCollection<AttributeItem> Attributes { get => attributes; }

        public ReactiveCommand<Unit, Unit> AddFirstAttr { get; }

        private void FuncAddFirstAttr() => attributes.Insert(0, new AttributeItem(this));
        public void FuncAddNextAttr(AttributeItem item) => attributes.Insert(attributes.IndexOf(item) + 1, new AttributeItem(this));
        public void FuncRemoveAttr(AttributeItem item) => attributes.Remove(item);

        readonly ObservableCollection<MethodItem> methods = new();
        public ObservableCollection<MethodItem> Methods { get => methods; }

        public ReactiveCommand<Unit, Unit> AddFirstMethod { get; }

        private void FuncAddFirstMethod() => methods.Insert(0, new MethodItem(this));
        public void FuncAddNextMethod(MethodItem item) => methods.Insert(methods.IndexOf(item) + 1, new MethodItem(this));
        public void FuncRemoveMethod(MethodItem item) => methods.Remove(item);

        private static readonly string[] stereos = new string[] { "static", "abstract" };

        private void FuncApply() {
            StringBuilder sb = new();
            sb.Append($"{"-+#~"[access]} {name}");
            if (stereo != 0) {
                sb.Append(' ');
                sb.Append(stereos[stereo - 1]);
            }
            foreach (var item in attributes) sb.Append("\n" + item);
            foreach (var item in methods) sb.Append("\n" + item);
            Log.Write(sb.ToString());
            FuncClose();
        }
        private void FuncClose() {
            if (menu == null) return;
            menu.Close();
            menu = null;
        }
        private void FuncClear() {
            Name = "May be ..";
            stereo = 0;
            access = 0;
            attributes.Clear();
            methods.Clear();
            this.RaisePropertyChanged(nameof(Stereo_1));
            this.RaisePropertyChanged(nameof(Access_1));
        }

        public ReactiveCommand<Unit, Unit> Apply { get; }
        public ReactiveCommand<Unit, Unit> Close { get; }
        public ReactiveCommand<Unit, Unit> Clear { get; }
    }
}