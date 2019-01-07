using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;
using System.IO;

namespace ThreadPoolReadWait
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private int completedCount;

        private int taskCount;

        private bool isRunning;

        private object obj = new object();

        private FileStream s;

        private DateTime beginDate;

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                WriteMsg("请停止后再开始");
                return;
            }

            isRunning = true;

            completedCount = 0;

            taskCount = int.Parse(txtTaskCount.Text) * 10000;

            s = File.Open("aa.txt", FileMode.Open);

            Thread thread = new Thread(Test);
            thread.Start();
        }

        private void Test()
        {

            WriteMsg("已开始");

            beginDate = DateTime.Now;

            for (int i = 0; i < taskCount; i++)
            {
                ThreadPool.QueueUserWorkItem(Read);
            }

        }

        private void Read(object o)
        {
            byte[] b = new byte[2048];

            Task<int> t = s.ReadAsync(b, 0, 2048);

            //  ThreadPool 里不能阻塞（Block）， 所以不能使用 Wait()
            //  所以，这个测试是不成立的。 
            //  如果使用了 Wait()，程序就会定在那里，没有结果，好像是死锁那样。
            t.Wait();

            lock (obj)
            {
                completedCount++;
            }

            if (completedCount >= taskCount)
            {
                s.Dispose();

                WriteMsg("已完成，共执行 " + taskCount + " 个任务，耗时 " + (DateTime.Now - beginDate).TotalSeconds.ToString() + " 秒");

                isRunning = false;
            }
        }

        private void WriteMsg(string msg)
        {
            txtMsg.AppendText(DateTime.Now.ToString("HH:mm:ss.fff") + " " + msg + "\r\n");
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtMsg.Clear();
        }
    }
}
