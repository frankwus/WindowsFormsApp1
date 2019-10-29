using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using mUtilities.Data;
using System.Threading; 
namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        public Form2() {
            InitializeComponent();
        }
        private void Form2_Load(object sender, EventArgs e) {
            Task t = Task.Run(() => {
                Random rnd = new Random();
                long sum = 0;
                int n = 500;
                for (int ctr = 1; ctr <= n; ctr++) {
                    int number = rnd.Next(0, 101);
                    sum += number;
                }

            });
            TimeSpan ts = TimeSpan.FromMilliseconds(150);
            if (!t.Wait(ts))
                this.SetText("The timeout interval elapsed.");
 
        return; 
        // test1();return; 
        Task.Run(() => test()).Wait();
            this.SetText("test1sdfds1");
        }
         void test() {
          //  Thread.Sleep(2000);
            this.SetText("don311");
        }
        async void test1() {
            this.SetText("test1");
            await test2();
            this.SetText("test11");
        }
        Task test2() {
            return Task.Factory.StartNew(() => {
                Thread.Sleep(2000);
                this.SetText("test2"); 
            });
        }
        delegate void SetTextCallback(string text);
        private void SetText(string text) {
            if(this.textBox1.InvokeRequired) {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            } else {
                this.textBox1.Text = this.textBox1.Text + Environment.NewLine + text;
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {

        }
    }
}
