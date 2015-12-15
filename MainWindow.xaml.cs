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
                c = int.Parse(v.TstatesCount.Split('-').First().ToString());
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
                label.Margin = new Thickness(i, 0, drawingSheet.ActualWidth - i, drawingSheet.ActualHeight - 350);
                var line = new Line();
                if (flag && a == red)
                    line.Stroke = Brushes.Red;
                else
                    line.Stroke = Brushes.LightSteelBlue;
                line.X1 = i;
                line.Y1 = j;
                line.X2 = i;
                line.Y2 = 360;
                drawingSheet.Children.Add(line);
                if (a < c)
                    drawingSheet.Children.Add(label);
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
                var l1 = new Line();
                var l2 = new Line();
                var l3 = new Line();
                var l4 = new Line();
                var l5 = new Line();
                l1.Stroke = Brushes.LightSteelBlue;
                l2.Stroke = l1.Stroke;
                l3.Stroke = l1.Stroke;
                l4.Stroke = l1.Stroke;
                l5.Stroke = l1.Stroke;
                l1.X1 = x;
                l1.Y1 = y;
                l1.X2 = x + shift + shift / 4;
                l1.Y2 = y;
                l2.X1 = l1.X2;
                l2.Y1 = l1.Y2;
                l2.X2 = l1.X2 + shift / 12;
                l2.Y2 = y + 40;
                l3.X1 = l2.X2;
                l3.Y1 = l2.Y2;
                l3.X2 = l2.X2 + shift;
                l3.Y2 = l3.Y1;
                l4.X1 = l3.X2;
                l4.Y1 = l3.Y2;
                l4.X2 = l3.X2 + shift / 12;
                l4.Y2 = y;
                l5.X1 = l4.X2;
                l5.Y1 = l4.Y2;
                l5.X2 = x + shift * length;
                l5.Y2 = l5.Y1;
                drawingSheet.Children.Add(l1);
                drawingSheet.Children.Add(l2);
                drawingSheet.Children.Add(l3);
                drawingSheet.Children.Add(l4);
                drawingSheet.Children.Add(l5);
            }
            else
            {
                var l = new Line();
                l.Stroke = Brushes.LightSteelBlue;
                l.X1 = x;
                l.Y1 = y;
                l.X2 = x + shift * length;
                l.Y2 = y;
                drawingSheet.Children.Add(l);
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
            var l1 = new Line();
            var l2 = new Line();
            var l3 = new Line();
            l1.Stroke = Brushes.LightSteelBlue;
            l2.Stroke = l1.Stroke;
            l3.Stroke = l1.Stroke;
            if (cycles[i] == "I" || cycles[i] == "O")
            {
                l1.X1 = x;
                l1.Y1 = y + 40;
                l1.X2 = x + shift / 12;
                l1.Y2 = y;
                l2.X1 = l1.X2;
                l2.Y1 = l1.Y2;
                l2.X2 = x + length * shift - shift / 12;
                l2.Y2 = l1.Y2;
                l3.X1 = l2.X2;
                l3.Y1 = l2.Y2;
                l3.X2 = l2.X2 + shift / 12;
                l3.Y2 = y + 40;
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
                var l1 = new Line();
                var l2 = new Line();
                var l3 = new Line();
                var l4 = new Line();
                var l5 = new Line();
                l1.Stroke = Brushes.LightSteelBlue;
                l2.Stroke = l1.Stroke;
                l3.Stroke = l1.Stroke;
                l4.Stroke = l1.Stroke;
                l5.Stroke = l1.Stroke;
                l1.X1 = x;
                l1.Y1 = y;
                l1.X2 = x + shift + shift / 4;
                l1.Y2 = y;
                l2.X1 = l1.X2;
                l2.Y1 = l1.Y2;
                l2.X2 = l1.X2 + shift / 12;
                l2.Y2 = y + 40;
                l3.X1 = l2.X2;
                l3.Y1 = l2.Y2;
                l3.X2 = l2.X2 + shift;
                l3.Y2 = l3.Y1;
                l4.X1 = l3.X2;
                l4.Y1 = l3.Y2;
                l4.X2 = l3.X2 + shift / 12;
                l4.Y2 = y;
                l5.X1 = l4.X2;
                l5.Y1 = l4.Y2;
                l5.X2 = x + shift * length;
                l5.Y2 = l5.Y1;
                drawingSheet.Children.Add(l1);
                drawingSheet.Children.Add(l2);
                drawingSheet.Children.Add(l3);
                drawingSheet.Children.Add(l4);
                drawingSheet.Children.Add(l5);
            }
            else
            {
                var l = new Line();
                l.Stroke = Brushes.LightSteelBlue;
                l.X1 = x;
                l.Y1 = y;
                l.X2 = x + shift * length;
                l.Y2 = y;
                drawingSheet.Children.Add(l);
            }
        }

        private void DrawALE(int i, Instruction v, ref Canvas drawingSheet)
        {
            var l1 = new Line();
            var l2 = new Line();
            var l3 = new Line();
            var l4 = new Line();
            l1.Stroke = Brushes.LightSteelBlue;
            l2.Stroke = l1.Stroke;
            l3.Stroke = l1.Stroke;
            l4.Stroke = l1.Stroke;
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
            l1.X1 = x;
            l1.Y1 = y + 40;
            l1.X2 = x + shift / 12;
            l1.Y2 = y;
            l2.X1 = l1.X2;
            l2.Y1 = l1.Y2;
            l2.X2 = x + shift / 2;
            l2.Y2 = y;
            l3.X1 = l2.X2;
            l3.Y1 = l2.Y2;
            l3.X2 = l2.X2 + shift / 12;
            l3.Y2 = y + 40;
            l4.X1 = l3.X2;
            l4.Y1 = l3.Y2;
            l4.X2 = x + length * shift;
            l4.Y2 = l4.Y1;
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
                //Declaration and stroke
                var xl1 = new Line();
                var xl2 = new Line();
                var xline1 = new Line();
                var xline2 = new Line();
                var x1 = new Line();
                var x2 = new Line();
                var straight = new Line();
                xl1.Stroke = Brushes.LightSteelBlue;
                xl2.Stroke = xl1.Stroke;
                xline1.Stroke = xl1.Stroke;
                xline2.Stroke = xl1.Stroke;
                x1.Stroke = xl1.Stroke;
                x2.Stroke = xl1.Stroke;
                straight.Stroke = xl1.Stroke;
                //Vector part
                xl1.X1 = x;
                xl1.Y1 = y;
                xl1.X2 = x + shift / 12;
                xl1.Y2 = y + 40;
                xline1.X1 = xl1.X2;
                xline1.Y1 = xl1.Y2;
                xline1.X2 = x + shift;
                xline1.Y2 = xl1.Y2;
                xl2.X1 = x;
                xl2.Y1 = xl1.Y2;
                xl2.X2 = x + shift / 12;
                xl2.Y2 = y;
                xline2.X1 = xl2.X2;
                xline2.Y1 = xl2.Y2;
                xline2.X2 = x + shift;
                xline2.Y2 = xl2.Y2;
                x1.X1 = xline1.X2;
                x1.Y1 = xline1.Y2;
                x1.X2 = xline1.X2 + shift / 24;
                x1.Y2 = y + 20;
                x2.X1 = xline2.X2;
                x2.Y1 = xline2.Y2;
                x2.X2 = xline2.X2 + shift / 24;
                x2.Y2 = y + 20;
                straight.X1 = x2.X2;
                straight.Y1 = y + 20;
                straight.X2 = x + shift + (shift / 3) + shift / 12;
                straight.Y2 = straight.Y1;
                //Adding to canvas
                if (k == 0 && i == 0)
                {
                    drawingSheet.Children.Add(xl1);
                    drawingSheet.Children.Add(xl2);
                }
                else
                {
                    var l1 = new Line();
                    var l2 = new Line();
                    l1.Stroke = xl1.Stroke;
                    l2.Stroke = xl1.Stroke;
                    l1.X1 = x + shift / 24;
                    l1.Y1 = y + 20;
                    l1.X2 = x + shift / 12;
                    l1.Y2 = y;
                    l2.X1 = l1.X1;
                    l2.Y1 = y + 20;
                    l2.X2 = l1.X2;
                    l2.Y2 = y + 40;
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
            var line1 = new Line();
            var l1 = new Line();
            var line2 = new Line();
            var l2 = new Line();
            line1.Stroke = Brushes.LightSteelBlue;
            l1.Stroke = line1.Stroke;
            line2.Stroke = l1.Stroke;
            l2.Stroke = l1.Stroke;
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
            line1.X1 = x;
            line1.Y1 = y;
            line1.X2 = x + shift / 12;
            line1.Y2 = y + 40;
            l1.X1 = line1.X2;
            l1.Y1 = line1.Y2;
            l1.X2 = x + length * shift;
            l1.Y2 = l1.Y1;
            line2.X1 = x;
            line2.Y1 = line1.Y2;
            line2.X2 = x + shift / 12;
            line2.Y2 = y;
            l2.X1 = line2.X2;
            l2.Y1 = line2.Y2;
            l2.X2 = x + length * shift;
            l2.Y2 = l2.Y1;
            drawingSheet.Children.Add(line1);
            drawingSheet.Children.Add(line2);
            drawingSheet.Children.Add(l1);
            drawingSheet.Children.Add(l2);
        }
    }
}