using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

namespace XlApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //.+ is greedy, so it will match as many characters as possible before stopping. Change it to .+? and the match will end as soon as possible.
            patternBox.Text = @"xL\""(?<val>.+?)\""";
        }

        static Regex idConvertPattern = new Regex(@"[\\ \.-]");

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            outBox.Items.Clear();

            Dictionary<string, string> header_vars = new Dictionary<string, string>();

            var names = Directory.GetFiles(pathBox.Text, "*.cpp", SearchOption.AllDirectories);

            int total_replaced_count = 0;

            foreach (var path in names)
            {
                int replaced_count = 0;

                var name = Path.GetFileNameWithoutExtension(path);

                string text = File.ReadAllText(path);

                string replaced_text = Regex.Replace(text, patternBox.Text, match =>
                {
                    ++replaced_count;

                    var val = match.Groups["val"].Value;

                    var id = idConvertPattern.Replace(val, "_");

                    string key = $"{name}_{id}";

                    if (header_vars.ContainsKey(key))
                    {
                        System.Diagnostics.Debug.Assert(header_vars[key] == val);
                    }
                    else
                    {
                        header_vars.Add(key, val);
                    }

                    return $"Constants::NetworkDevicesAuditing::{key}";
                });

                if (replaced_count != 0)
                {
                    outBox.Items.Add($"Replaced {replaced_count} occurrences in '{path}'.");

                    File.WriteAllText(path, replaced_text);
                }

                total_replaced_count += replaced_count;
            }

            outBox.Items.Add($"Total replacemetns: {total_replaced_count}.");

            StringBuilder builder = new StringBuilder();

            foreach (var hv in header_vars)
            {
                builder.AppendLine($"static const wchar_t {hv.Key}[] = L\"{hv.Value}\";");
            }

            headerBox.Text = builder.ToString();

            statusLabel.Content = "Done";
        }
    }
}
