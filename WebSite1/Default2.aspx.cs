using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Default2 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var t1 = new Thread(ThreadMain);
        t1.Name = "MyNewThread1";
        t1.IsBackground = true;
        t1.Priority = ThreadPriority.Normal;
        t1.Start();
    }

    static void ThreadMain()
    {
        Thread.Sleep(30000);
        string file = "\\\\localhost\\c$\\dev\\thread.txt"; // 192.168.2.15\c$\Program Files\Lexicom\outbox\.edi

        // ToDO: Append or Create new file? - FileName in server? - Full Path?
        using (StreamWriter writer = File.AppendText(Path.GetFullPath(file)))
        {
            writer.Write("Thread {0} started", Thread.CurrentThread.Name);

            writer.Write("Thread {0} completed", Thread.CurrentThread.Name);
        }
    }
}