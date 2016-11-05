using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace PP_HW2_IDM2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static byte[] fileByte;
        private static int percentProgressOfEachConnection;
        // private static int percentProgressOfAllConnection;
        private static double speedOfEachlConnection;
        private delegate void UpdateProgessCallback1(Int64 BytesRead, Int64 TotalBytes, Int64 indexLabel, Stopwatch sw);
        private delegate void UpdateProgessCallback2(Int64 percent , Int64 speed);
        static int numConnection = 5;
        Task<byte[]>[] task = new Task<byte[]>[numConnection];
        static int ilabel2;
        static int gap1 = 156;
        static int gap2 = 105;
        static int gap3 = 84;
      
        long length;
        static int percent = 0;

        int[] speedConnection = new int[numConnection];
        static int speed = 0;
        

        private void updateProgress1(Int64 byteRead, Int64 totalBytes, Int64 indexLabel, Stopwatch sw)
        {
            // Calculate the download progress in percentages
            percentProgressOfEachConnection = (Convert.ToInt32((byteRead * 100) / totalBytes));
            speedOfEachlConnection = Convert.ToInt32(((byteRead) / (sw.Elapsed.TotalSeconds)) / 1000);

            progressBar[indexLabel].Value = percentProgressOfEachConnection;
            speedConnection[indexLabel] =(int)speedOfEachlConnection;
            // Display the current progress on the form
            label[indexLabel].Text = "Downloaded Connection " + indexLabel.ToString() + " : "
                + percentProgressOfEachConnection.ToString() + " % "
                + "  speed connection :" + speedOfEachlConnection.ToString() + " kbit/s";
            this.Invoke(new UpdateProgessCallback2(this.updateProgress2), new object[] { percent , speed });

        }
        private void updateProgress2(Int64 percent , Int64 speed)
        {
            for (int i = 0; i < numConnection; i++)
            {
                // Calculate the download progress in percentages in all connection
                percent += (int)(((double)progressBar[i].Value));
                speed += speedConnection[i];
            }
            progressBar1.Value = Convert.ToInt32(percent / numConnection);
            // Display the current progress on the form label
            label4.Text = "Downloaded all Connection :" + (percent / numConnection).ToString() + " % ";
            label5.Text = "Speed all Connection :" + (speed / numConnection).ToString() + " kbit/s";
        }

        byte[] download(object obj)
        {
            try
            {
                HttpWebRequest webRequest;
                HttpWebResponse webResponse;
                Stream strResponse;
                object[] obj1 = (object[])obj;
                long from = (long)obj1[0];
                long offset = (long)obj1[1];
                int ilabel = (int)obj1[2];

                // Create a request to the file we are downloading
                webRequest = (HttpWebRequest)WebRequest.Create(textBox1.Text);

                // Set default authentication for retrieving the file
                webRequest.Credentials = CredentialCache.DefaultCredentials;

                //Range file requsest from , offset
                webRequest.AddRange((int)from, (int)offset);
                // Retrieve the response from the server
                webResponse = (HttpWebResponse)webRequest.GetResponse();

                //Store response in file stream
                strResponse = webResponse.GetResponseStream();
                Int64 fileSize = webResponse.ContentLength;
                MemoryStream msLocal = new MemoryStream();

                int bytesSize = 0;
                byte[] downBuffer = new byte[2048];
                Stopwatch sw = new Stopwatch();
                sw.Start();
                bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length);
                sw.Stop();
                while (bytesSize > 0)
                {
                    // Write the data from the buffer to the memory drive
                    msLocal.Write(downBuffer, 0, bytesSize);
                    this.Invoke(new UpdateProgessCallback1(this.updateProgress1), new object[] { msLocal.Length, fileSize, ilabel, sw });
                    sw.Start();
                    bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length);
                    sw.Stop();
                }
                fileByte = msLocal.ToArray();

                // updateProgress(msLocal.Length, fileSize);
                webResponse.Close();
                strResponse.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return fileByte;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                HttpWebRequest webRequest;
                HttpWebResponse webResponse;
                Uri uri = new Uri(textBox1.Text);

                string filename = Path.GetFileName(uri.LocalPath);

                webRequest = (HttpWebRequest)WebRequest.Create(textBox1.Text);
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                webResponse = (HttpWebResponse)webRequest.GetResponse();
                length = webResponse.ContentLength;
                webResponse.Close();
                //int numConnection = 3;
                //Task<byte[]>[] task = new Task<byte[]>[numConnection];
                object[] obj = new object[numConnection][];
                label3.Visible = true;
                label3.Text = "Download Starting...";
                for (int i = 0; i < numConnection - 1; i++)
                {
                    ilabel2 = i;
                    obj[i] = new object[] { length / numConnection * i, (length / numConnection * (i + 1)) - 1, i };

                    label[i] = new Label();
                    this.label[i].AutoSize = true;
                    this.label[i].Location = new System.Drawing.Point(224, gap1);
                    this.label[i].Name = "label" + (i + 4).ToString();
                    this.label[i].Size = new System.Drawing.Size(20, 13);
                    this.label[i].TabIndex = i + 4;
                    this.label[i].Text = "0";
                    this.Controls.Add(label[i]);
                    gap1 += 10;

                    progressBar[i] = new ProgressBar();
                    this.progressBar[i].Location = new System.Drawing.Point(gap3, gap2);
                    this.progressBar[i].Name = "progressBar" + (i + 2).ToString();
                    this.progressBar[i].Size = new System.Drawing.Size(471 / numConnection, 23);
                    this.progressBar[i].TabIndex = 6;
                    this.Controls.Add(progressBar[i]);
                    gap3 += 84;
                    task[i] = new Task<byte[]>(download, obj[i]);
                    task[i].Start();

                }
                ilabel2 += 1;
                label[ilabel2] = new Label();
                this.label[ilabel2].AutoSize = true;
                this.label[ilabel2].Location = new System.Drawing.Point(224, gap1);
                this.label[ilabel2].Name = "label" + (ilabel2 + 3).ToString();
                this.label[ilabel2].Size = new System.Drawing.Size(20, 13);
                this.label[ilabel2].TabIndex = ilabel2 + 4;
                this.label[ilabel2].Text = "0";
                this.Controls.Add(label[ilabel2]);

                progressBar[ilabel2] = new ProgressBar();
                this.progressBar[ilabel2].Location = new System.Drawing.Point(gap3, gap2);
                this.progressBar[ilabel2].Name = "progressBar" + (ilabel2 + 2).ToString();
                this.progressBar[ilabel2].Size = new System.Drawing.Size(471 / numConnection, 23);
                this.progressBar[ilabel2].TabIndex = 6;
                this.Controls.Add(progressBar[ilabel2]);

                obj[numConnection - 1] = new object[] { length / numConnection * (numConnection - 1), length - 1, ilabel2 };
                task[numConnection - 1] = new Task<byte[]>(download, obj[numConnection - 1]);
                task[numConnection - 1].Start();

                this.label4.AutoSize = true;
                this.label4.Location = new System.Drawing.Point(224, gap1 + 10);
                this.label4.Name = "label4";
                this.label4.Size = new System.Drawing.Size(20, 13);
                this.label4.TabIndex = ilabel2 + 5;
                this.Controls.Add(label4);

                this.label5.AutoSize = true;
                this.label5.Location = new System.Drawing.Point(224, gap1 + 20);
                this.label5.Name = "label5";
                this.label5.Size = new System.Drawing.Size(20, 13);
                this.label5.TabIndex = ilabel2 + 6;
                this.Controls.Add(label5);

                Task t1 = new Task(() =>
                    {
                        Task.WaitAll(task);
                        FileStream strLocal;
                        strLocal = new FileStream(textBox2.Text + ":\\" + filename,
                                  FileMode.Create, FileAccess.Write,
                                  FileShare.ReadWrite);

                        byte[] fileByte = new byte[length];
                        for (int i = 0; i < numConnection; i++)
                        {
                            task[i].Result.CopyTo(fileByte, length / numConnection * i);
                        }
                        strLocal.Write(fileByte, 0, fileByte.Length);
                        strLocal.Close();
                    });
                t1.Start();

            }
            catch (Exception ae)
            {
                MessageBox.Show(ae.Message);
            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}

