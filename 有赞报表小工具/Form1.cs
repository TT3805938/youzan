using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security.Cryptography;

namespace 有赞报表小工具
{
    public partial class Form1 : Form
    {
        public static Form1 ff;
        public Form1()
        {
            InitializeComponent();
            ff = this;
        }
        public static string resultTxt="";
        private void Button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.ShowDialog();
            this.txtSrc1.Text = file.FileName;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.ShowDialog();
            this.txtSrc2.Text = file.FileName;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            txtResult.Text = "";
            resultTxt = "";
            string path1 = txtSrc1.Text.Trim();
            string path2 = txtSrc2.Text.Trim();
            string path3 = txtRef.Text.Trim();
            string path4 = txtFreight.Text.Trim();
            if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2) || string.IsNullOrEmpty(path3) || string.IsNullOrEmpty(path4)) { MessageBox.Show("参数非法!");return; }
            RunPythonScript("excel-python\\excel.py","",new string[] {path1,path2, path3,path4 });
            txtResult.Text = resultTxt;
        }
        //调用python核心代码
        public static void RunPythonScript(string sArgName, string args = "", params string[] teps)
        {
            Process p = new Process();
            string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + sArgName;// 获得python文件的绝对路径（将文件放在c#的debug文件夹中可以这样操作）
            //path = @"C:\Users\user\Desktop\test\" + sArgName;//(因为我没放debug下，所以直接写的绝对路径,替换掉上面的路径了)
            p.StartInfo.FileName = @"python.exe";//没有配环境变量的话，可以像我这样写python.exe的绝对路径。如果配了，直接写"python.exe"即可1
            string sArguments = path;
            foreach (string sigstr in teps)
            {
                sArguments += " " + sigstr;//传递参数
            }
            sArguments += " " + args;
            p.StartInfo.Arguments = sArguments;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            p.Start();
            p.BeginOutputReadLine();
            p.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
            Console.ReadLine();
            p.WaitForExit();
        }
        //输出打印的信息
        static void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                AppendText(e.Data + Environment.NewLine);
            }
        }
        public delegate void AppendTextCallback(string text);
        public static void AppendText(string text)
        {
            Console.WriteLine(text);//此处在控制台输出.py文件print的结果
            resultTxt += text;
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.ShowDialog();
            this.txtRef.Text = file.FileName;
        }
        private void Button7_Click(object sender, EventArgs e)
        {
            txtResult.Text = "";
            resultTxt = "";
            string path3 = txtSrc1.Text.Trim();
            string path1 = txtSrc2.Text.Trim();
            string path2 = txtRef.Text.Trim();
            if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2) || string.IsNullOrEmpty(path3)) { MessageBox.Show("参数非法!"); return; }
            //if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2)) { MessageBox.Show("参数非法!");return; }
            RunPythonScript("excel-python\\excel_2.py", "", new string[] { path1, path2,path3 });
            txtResult.Text = resultTxt;
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.ShowDialog();
            this.txtFreight.Text = file.FileName;
        }
    }
}
