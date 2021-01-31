using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           // this.mychrome.MenuHandler = new MenuHandler();  //去掉右键菜单  需要实现 IContextMenuHandler 这个接口
            this.mychrome.Address = @"https://localhost:2002/api/Identity";
            this.mychrome.RegisterJsObject("JsObj", new CallbackObjectForJs());
            //this.wb.Navigate("");
        }
    }
}
