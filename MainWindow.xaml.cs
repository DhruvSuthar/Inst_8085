using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.IO;

namespace Inst8085
{
    public partial class MainWindow : Window
    {
        private List<Instruction> InstructionsList { get; set; }
        private List<string> displayList { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            if (InstructionsList == null)
            {
                InstructionsList = new List<Instruction>();
                displayList = new List<string>();
                StreamReader sr = new StreamReader("data.json");
                string data = sr.ReadToEnd();
                InstructionsList = JsonConvert.DeserializeObject<List<Instruction>>(data);
                foreach (Instruction item in InstructionsList)
                {
                    displayList.Add(item.Name);
                }
            }
            listBox.ItemsSource = displayList;
            getTimingDiagram.Background = Brushes.White;
            getTimingDiagram.Foreground = Brushes.Black;
            getTimingDiagram.BorderThickness = new Thickness(2, 2, 2, 2);
            getTimingDiagram.BorderBrush = Brushes.Black;
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            drawingSheet.Children.Clear();
            TStatesCanvas.Children.Clear();
            MCycleCanvas.Children.Clear();
            var item = sender as ListView;
            var instr = InstructionsList[item.SelectedIndex];
            name.Content = instr.Name;
            inst.Content = instr.instruction;
            opCode.Content = instr.OpCode;
            operand.Content = instr.Operand;
            bytes.Content = instr.Bytes;
            mCycles.Content = instr.McyclesCount + " : " + instr.Mcycles;
            tStates.Content = instr.TstatesCount;
            adMode.Content = instr.AddressingMode;
            s.Content = instr.flgs.S;
            cy.Content = instr.flgs.CY;
            p.Content = instr.flgs.P;
            z.Content = instr.flgs.Z;
            ac.Content = instr.flgs.AC;
            hexCode.Content = instr.hexCode;
            grp.Content = instr.Group;
            if (!(this.listBox.SelectedIndex == -1) && drawingSheet.Children.Count == 0)
            {
                DrawDiagram(ref drawingSheet, InstructionsList[listBox.SelectedIndex]);
            }
        }

        private void DrawDiagram(ref Canvas drawingSheet, Instruction v)
        {
            DrawLines(v, ref drawingSheet);
            DeawCLK(v, ref drawingSheet);
            int c = 0;
            if (v.McyclesCount.Contains('-'))
            {
                c = int.Parse(v.McyclesCount.Split('-').First().ToString());
            }
            else
                c = int.Parse(v.McyclesCount);
            for (int i = 0; i < c; i++)
            {
                DrawMCycle(i, v, ref drawingSheet);
            }
        }

        private void DeawCLK(Instruction v, ref Canvas drawingSheet)
        {
            double x = 80;
            double y = 10;
            drawingSheet.Children.Add(getLabel(y, "CLK"));
            int c = 0;
            if (v.TstatesCount.Contains('-'))
            {
                c = int.Parse(v.TstatesCount.Split('-').Last().ToString());
            }
            else
                c = int.Parse(v.TstatesCount);
            double shift = (drawingSheet.ActualWidth - 60) / c;
            for (int i = 0; i < c * 2; i++)
            {
                var line1 = new Line();
                line1.Stroke = Brushes.LightSteelBlue;
                line1.X1 = x;
                line1.Y1 = y;
                line1.X2 = (x += shift / 12);
                if (i % 2 == 0)
                    line1.Y2 = (y += 40);
                else
                    line1.Y2 = (y -= 40);
                var line2 = new Line();
                line2.Stroke = Brushes.LightSteelBlue;
                line2.X1 = x;
                line2.Y1 = y;
                line2.X2 = (x += 5 * shift / 12);
                line2.Y2 = y;
                drawingSheet.Children.Add(line1);
                drawingSheet.Children.Add(line2);
            }
        }

        private Label getLabel(double y, string content)
        {
            Label l = new Label();
            l.Content = content;
            l.FontSize = 15;
            l.Foreground = Brushes.Blue;
            l.Margin = new Thickness(5, y + 5, drawingSheet.ActualWidth - 60, 0);
            return l;
        }

        private Line getLine(double x1, double y1, double x2, double y2, SolidColorBrush brush)
        {
            Line l = new Line();
            l.X1 = x1;
            l.Y1 = y1;
            l.X2 = x2;
            l.Y2 = y2;
            l.Stroke = brush;
            return l;
        }

        private void DrawLines(Instruction v, ref Canvas drawingSheet)
        {
            double i = 80;
            double j = 10;
            bool flag;
            int c = 0;
            int red = 0;
            if (v.TstatesCount.Contains('-'))
            {
                flag = true;
                c = int.Parse(v.TstatesCount.Split('-').Last().ToString());
                red = int.Parse(v.TstatesCount.Split('-').First().ToString());
            }
            else
            {
                flag = false;
                c = int.Parse(v.TstatesCount);
            }
            for (int a = 0; a <= c; a++)
            {
                var label = new Label();
                label.Content = "T" + (a + 1);
                label.Margin = new Thickness(i + (drawingSheet.ActualWidth - 60) / (c * 2), 0, TStatesCanvas.ActualWidth - i, 15);
                var line = new Line();
                if (flag && a == red)
                {
                    drawingSheet.Children.Add(getLine(i, j, i, 360, Brushes.Red));
                    TStatesCanvas.Children.Add(getLine(i,j,i,20,Brushes.Red));
                }
                else
                {
                    drawingSheet.Children.Add(getLine(i, j, i, 360, Brushes.LightBlue));
                    TStatesCanvas.Children.Add(getLine(i, j, i, 20, Brushes.LightSteelBlue));
                }
                if (a < c)
                    TStatesCanvas.Children.Add(label);
                i += (drawingSheet.ActualWidth - 60) / c;
            }


        }

        private void DrawMCycle(int i, Instruction v, ref Canvas drawingSheet)
        {
            DrawAddress(i, v, ref drawingSheet);
            DrawADLines(i, v, ref drawingSheet);
            DrawALE(i, v, ref drawingSheet);
            DrawIOM(i, v, ref drawingSheet);
            DrawRD(i, v, ref drawingSheet);
            DrawWR(i, v, ref drawingSheet);
        }

        private void DrawWR(int i, Instruction v, ref Canvas drawingSheet)
        {
            double x = 80;
            double y = 310;
            drawingSheet.Children.Add(getLabel(y, "WR"));
            int c = 0;
            if (v.TstatesCount.Contains('-'))
            {
                c = int.Parse(v.TstatesCount.Split('-').Last().ToString());
            }
            else
                c = int.Parse(v.TstatesCount);
            double shift = (drawingSheet.ActualWidth - 60) / c;
            double length;
            var cycles = v.Mcycles.Split(' ');
            if (cycles[i] == "S") length = 6;
            else if (cycles[i] == "F") length = 4;
            else length = 3;
            for (int j = 0; j < i; j++)
            {
                if (cycles[j] == "S") x += 6 * shift;
                else if (cycles[j] == "F") x += 4 * shift;
                else x += 3 * shift;
            }
            if (cycles[i] == "W" || cycles[i] == "O")
            {
                var l1 = getLine(x, y, x + shift + shift / 4, y, Brushes.LightSteelBlue);
                var l2 = getLine(l1.X2, l1.Y2, l1.X2 + shift / 12, y + 40, Brushes.LightSteelBlue);
                var l3 = getLine(l2.X2, l2.Y2, l2.X2 + shift, l2.Y2, Brushes.LightSteelBlue);
                var l4 = getLine(l3.X2, l3.Y2, l3.X2 + shift / 12, y, Brushes.LightSteelBlue);
                var l5 = getLine(l4.X2, l4.Y2, x + shift * length, l4.Y2, Brushes.LightSteelBlue);
                
                drawingSheet.Children.Add(l1);
                drawingSheet.Children.Add(l2);
                drawingSheet.Children.Add(l3);
                drawingSheet.Children.Add(l4);
                drawingSheet.Children.Add(l5);
            }
            else
            {
                drawingSheet.Children.Add(getLine(x, y, x + shift * length, y, Brushes.LightSteelBlue));
            }
        }

        private void DrawIOM(int i, Instruction v, ref Canvas drawingSheet)
        {
            double x = 80;
            double y = 210;
            drawingSheet.Children.Add(getLabel(y, "IO/M"));
            int c = 0;
            if (v.TstatesCount.Contains('-'))
            {
                c = int.Parse(v.TstatesCount.Split('-').Last().ToString());
            }
            else
                c = int.Parse(v.TstatesCount);
            double shift = (drawingSheet.ActualWidth - 60) / c;
            double length;
            var cycles = v.Mcycles.Split(' ');
            if (cycles[i] == "S") length = 6;
            else if (cycles[i] == "F") length = 4;
            else length = 3;
            for (int j = 0; j < i; j++)
            {
                if (cycles[j] == "S") x += 6 * shift;
                else if (cycles[j] == "F") x += 4 * shift;
                else x += 3 * shift;
            }
            var l1 = getLine(x, y + 40, x + shift / 12, y, Brushes.LightSteelBlue);
            var l2 = getLine(l1.X2, l1.Y2, x + length * shift - shift / 12, l1.Y2, Brushes.LightSteelBlue);
            var l3 = getLine(l2.X2, l2.Y2, l2.X2 + shift / 12, y + 40, Brushes.LightSteelBlue);
            l1.Stroke = Brushes.LightSteelBlue;
            l2.Stroke = l1.Stroke;
            l3.Stroke = l1.Stroke;
            if (cycles[i] == "I" || cycles[i] == "O")
            {
                drawingSheet.Children.Add(l1);
                drawingSheet.Children.Add(l2);
                drawingSheet.Children.Add(l3);
            }
            else
            {
                l1.X1 = x;
                l1.Y1 = y + 40;
                l1.X2 = x + length * shift;
                l1.Y2 = l1.Y1;
                drawingSheet.Children.Add(l1);
            }

        }

        private void DrawRD(int i, Instruction v, ref Canvas drawingSheet)
        {
            double x = 80;
            double y = 260;
            drawingSheet.Children.Add(getLabel(y, "RD"));
            int c = 0;
            if (v.TstatesCount.Contains('-'))
            {
                c = int.Parse(v.TstatesCount.Split('-').Last().ToString());
            }
            else
                c = int.Parse(v.TstatesCount);
            double shift = (drawingSheet.ActualWidth - 60) / c;
            double length;
            var cycles = v.Mcycles.Split(' ');
            if (cycles[i] == "S") length = 6;
            else if (cycles[i] == "F") length = 4;
            else length = 3;
            for (int j = 0; j < i; j++)
            {
                if (cycles[j] == "S") x += 6 * shift;
                else if (cycles[j] == "F") x += 4 * shift;
                else x += 3 * shift;
            }
            if (cycles[i] == "F" || cycles[i] == "R" || cycles[i] == "I" || cycles[i] == "S")
            {
                var l1 = getLine(x, y, x + shift + shift / 4, y, Brushes.LightSteelBlue);
                var l2 = getLine(l1.X2, l1.Y2, l1.X2 + shift / 12, y + 40, Brushes.LightSteelBlue);
                var l3 = getLine(l2.X2, l2.Y2, l2.X2 + shift, l2.Y2, Brushes.LightSteelBlue);
                var l4 = getLine(l3.X2, l3.Y2, l3.X2 + shift / 12, y, Brushes.LightSteelBlue);
                var l5 = getLine(l4.X2, l4.Y2, x + shift * length, l4.Y2, Brushes.LightSteelBlue);

                drawingSheet.Children.Add(l1);
                drawingSheet.Children.Add(l2);
                drawingSheet.Children.Add(l3);
                drawingSheet.Children.Add(l4);
                drawingSheet.Children.Add(l5);
            }
            else
            {
                drawingSheet.Children.Add(getLine(x, y, x + shift * length, y, Brushes.LightSteelBlue));
            }
        }

        private void DrawALE(int i, Instruction v, ref Canvas drawingSheet)
        {
            double x = 80;
            double y = 160;
            drawingSheet.Children.Add(getLabel(y, "ALE"));
            int c = 0;
            if (v.TstatesCount.Contains('-'))
            {
                c = int.Parse(v.TstatesCount.Split('-').Last().ToString());
            }
            else
                c = int.Parse(v.TstatesCount);
            double shift = (drawingSheet.ActualWidth - 60) / c;
            double length;
            var cycles = v.Mcycles.Split(' ');
            if (cycles[i] == "S") length = 6;
            else if (cycles[i] == "F") length = 4;
            else length = 3;
            for (int j = 0; j < i; j++)
            {
                if (cycles[j] == "S") x += 6 * shift;
                else if (cycles[j] == "F") x += 4 * shift;
                else x += 3 * shift;
            }
            var l1 = getLine(x, y + 40, x + shift / 12, y, Brushes.LightSteelBlue);
            var l2 = getLine(l1.X2, l1.Y2, x + shift / 2, y, Brushes.LightSteelBlue);
            var l3 = getLine(l2.X2, l2.Y2, l2.X2 + shift / 12, y + 40, Brushes.LightSteelBlue);
            var l4 = getLine(l3.X2, l3.Y2, x + length * shift, l3.Y2, Brushes.LightSteelBlue);

            drawingSheet.Children.Add(l1);
            drawingSheet.Children.Add(l2);
            drawingSheet.Children.Add(l3);
            drawingSheet.Children.Add(l4);
        }

        private void DrawADLines(int i, Instruction v, ref Canvas drawingSheet)
        {
            double x = 80;
            double y = 110;
            double x_i = 80;
            drawingSheet.Children.Add(getLabel(y, "AD0-AD7"));
            int c = 0;
            if (v.TstatesCount.Contains('-'))
            {
                c = int.Parse(v.TstatesCount.Split('-').Last().ToString());
            }
            else
                c = int.Parse(v.TstatesCount);
            double shift = (drawingSheet.ActualWidth - 60) / c;
            double length;
            var cycles = v.Mcycles.Split(' ');
            if (cycles[i] == "S") length = 6;
            else if (cycles[i] == "F") length = 4;
            else length = 3;
            for (int j = 0; j < i; j++)
            {
                if (cycles[j] == "S") { x += 6 * shift; x_i += 6 * shift; }
                else if (cycles[j] == "F") { x += 4 * shift; x_i += 4 * shift; }
                else { x += 3 * shift; x_i += 3 * shift; }
            }
            for (int k = 0; k < 2; k++)
            {
                var xl1 = getLine(x, y, x + shift / 12, y + 40, Brushes.LightSteelBlue);
                var xl2 = getLine(x, xl1.Y2, x + shift / 12, y, Brushes.LightSteelBlue);
                var xline1 = getLine(xl1.X2, xl1.Y2, x + shift, xl1.Y2, Brushes.LightSteelBlue);
                var xline2 = getLine(xl2.X2, xl2.Y2, x + shift, xl2.Y2, Brushes.LightSteelBlue);
                var x1 = getLine(xline1.X2, xline1.Y2, xline1.X2 + shift / 24, y + 20, Brushes.LightSteelBlue);
                var x2 = getLine(xline2.X2, xline2.Y2, xline2.X2 + shift / 24, y + 20, Brushes.LightSteelBlue);
                var straight = getLine(x2.X2, y + 20, x + shift + (shift / 3) + shift / 12, y + 20, Brushes.LightSteelBlue);
                if (k == 0 && i == 0)
                {
                    drawingSheet.Children.Add(xl1);
                    drawingSheet.Children.Add(xl2);
                }
                else
                {
                    var l1 = getLine(x + shift / 24, y + 20, x + shift / 12, y, Brushes.LightSteelBlue);
                    var l2 = getLine(l1.X1, y + 20, l1.X2, y + 40, Brushes.LightSteelBlue);
                    if (k != 0)
                        straight.X2 = x_i + shift * length + shift / 24;
                    drawingSheet.Children.Add(l1);
                    drawingSheet.Children.Add(l2);
                }
                drawingSheet.Children.Add(xline2);
                drawingSheet.Children.Add(xline1);
                drawingSheet.Children.Add(x1);
                drawingSheet.Children.Add(x2);
                drawingSheet.Children.Add(straight);
                x += shift + shift / 3 + shift / 24;
            }
        }

        private void DrawAddress(int i, Instruction v, ref Canvas drawingSheet)
        {
            int c = 0;
            if (v.TstatesCount.Contains('-'))
            {
                c = int.Parse(v.TstatesCount.Split('-').Last().ToString());
            }
            else
                c = int.Parse(v.TstatesCount);
            double shift = (drawingSheet.ActualWidth - 60) / c;
            double x = 80;
            double y = 60;
            drawingSheet.Children.Add(getLabel(y, "A8-A15"));
            double length;
            var cycles = v.Mcycles.Split(' ');
            if (cycles[i] == "S") length = 6;
            else if (cycles[i] == "F") length = 4;
            else length = 3;
            for (int j = 0; j < i; j++)
            {
                if (cycles[j] == "S") x += 6 * shift;
                else if (cycles[j] == "F") x += 4 * shift;
                else x += 3 * shift;
            }
            var line1 = getLine(x, y, x + shift / 12, y + 40, Brushes.LightSteelBlue);
            var l1 = getLine(line1.X2, line1.Y2, x + length * shift, line1.Y2, Brushes.LightSteelBlue);
            var line2 = getLine(x, line1.Y2, x + shift / 12, y, Brushes.LightSteelBlue);
            var l2 = getLine(line2.X2, line2.Y2, x + length * shift, line2.Y2, Brushes.LightSteelBlue);
            
            drawingSheet.Children.Add(line1);
            drawingSheet.Children.Add(line2);
            drawingSheet.Children.Add(l1);
            drawingSheet.Children.Add(l2);
        }
    }
}