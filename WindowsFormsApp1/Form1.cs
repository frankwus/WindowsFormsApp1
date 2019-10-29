using System;

using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using mUtilities.Data;
using System.IO;
using System.Net;
using System;

using System.Diagnostics;
using System.Threading;
using System.Security;
using System.Security.Principal;
using System.Collections;

using System.Collections.Generic;
using System.Configuration;

using System.Data;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Security.Authentication;
using System.Xml.Linq;
using System.Net;
namespace WindowsFormsApp1
{

    public partial class Form1 : Form
    {
        string FailedRigs = "";
        string Root = System.AppContext.BaseDirectory + @"../../";
        string Sql = "";
        StringBuilder SqlStringBuilder = new StringBuilder(); 
        public Form1() {
            InitializeComponent();
        }
        DataAccessor da;
        private void Form1_Load(object sender, EventArgs e) {
            return; 
            this.checkBox1_CheckedChanged(null, null);
            this.listBox1.SelectionMode = SelectionMode.MultiSimple; 
            this.listBox1.DataSource = File.ReadAllLines(Root + "all.txt", Encoding.UTF8);
            this.listBox1.SelectedIndex = -1; 
        }
        void Start() {
            //  string Root = @"../../"; 
            string[] arr = File.ReadAllLines(Root + "all.txt", Encoding.UTF8);
            string sql = this.textBox1.Text; // File.ReadAllText(Root + "sql.txt", Encoding.UTF8);
            this.FailedRigs = "";
            foreach (string rig in arr) {
                if (rig.Trim() == "")
                    continue;
                this.DoSql(rig, sql);
            }
            this.ShowMessage("Failed", this.FailedRigs);
            File.WriteAllText(Root + "rig.txt", this.FailedRigs);
        }
        void DoSql(string rig, string sql, bool showGrid = false) {
            string database = "irma";
            //if (rig.Contains("8505"))
            //    database = "enscoIrma_8505";
            string pwd = "Admin_01234!"; 
            if (rig.Contains("121") || rig.Contains("75") )
                pwd = "Admin_1234!"; 
            string con = "user id=irma;password="+pwd+"; Data Source=" + rig + " ;  Initial Catalog=" + database;
            if (rig == "") {
                con = "user id=IRMA;password=oasql02npirma;Data Source=ddr-oasql02np;  Initial Catalog=irma_uat";
                showGrid = true;
            }
            da = new DataAccessor(con);
            try {
                DataSet ds = null;
                string[] arr = sql.Split(new string[] { Environment.NewLine + "go" }, StringSplitOptions.None);

                foreach (string sql1 in arr)
                    ds = da.GetDataSet(sql1);
               // this.SaveDataToUat(ds, rig); return; 
                Application.DoEvents();
                if (showGrid) {
                    if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                        this.dataGridView1.DataSource = ds.Tables[0];
                } else {
                    string s = "";
                    foreach (DataTable dt in ds.Tables) {
                        foreach (DataRow dr in dt.Rows) {
                            s += "\n" + dr[0];
                        }
                    }
                    this.ShowMessage(rig, s);
                }
              //  this.FailedRigs += Environment.NewLine + rig + Environment.NewLine + "OK";
            } catch (Exception ex) {
                this.FailedRigs += Environment.NewLine + rig +Environment.NewLine+ex.Message ;
                this.ShowMessage(rig, ex.Message);
            }
        }
        void SaveDataToUat(DataSet ds, string rig ) {
            string s = "";
            try {
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    s += " insert RigPobExtract select getdate() ";
                    foreach (DataColumn dc in ds.Tables[0].Columns) {
                        string s1 = "null";
                        if (dr[dc] != DBNull.Value)
                            s1 = dr[dc].ToString().Replace("'", "''");
                        s += ", " + (s1 == "null" ? "null" : "'" + s1 + "'"); 
                    }
                    s += ", '" + rig + "'";
                }
                if (s != "") {
                    string con = "user id=IRMA;password=oasql02npirma;Data Source=ddr-oasql02np;  Initial Catalog=irma_uat";
                    this.da = new DataAccessor(con);
                    this.da.GetDataSet(s);
                }
            }catch(Exception ex) {
                this.ShowMessage(rig, s); 
            }

        }
        void ShowMessage(string rig, string s) {
            s = Environment.NewLine + Environment.NewLine + rig + Environment.NewLine + s;
            // this.textBox1.Text = this.textBox1.Text + s;
            this.textBox1.Text =Environment.NewLine + this.textBox1.Text+ s; 
        }

        private void button1_Click(object sender, EventArgs e) {
            this.FailedRigs = "";
            string sql = this.textBox1.Text;
            foreach (var item in this.listBox1.SelectedItems) {
                string rig = item.ToString();
                this.DoSql(rig, sql, true);
            }
            this.ShowMessage("Failed", this.FailedRigs); 
            this.SendEmail();
        }
        private void button2_Click(object sender, EventArgs e) {
            this.Start();
            this.SendEmail();
        }
        void SendEmail() {
            da = new DataAccessor("user id=IRMA;password=oasql02npirma;Data Source=ddr-oasql02np;  Initial Catalog=irma_uat");
            da.GetDataSet(" exec usp_sendEmail 'frank.wang@valaris.com', 'test', '" + this.textBox1.Text.Replace("'", "''") + "'");
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            bool selectAll = false;
            if (this.checkBox1.Checked)
                selectAll = true;

            listBox1.BeginUpdate();

            for (int i = 0; i < listBox1.Items.Count; i++)
                listBox1.SetSelected(i, selectAll);

            listBox1.EndUpdate();
        }

        private void UploadPhoto(bool isAttachment =false ) {
            DirectoryInfo di = new DirectoryInfo(@"c:\aa");
            string sql = "";
            int count = 0; 
            foreach(FileInfo fi in di.GetFiles("*.png")) {
                string guid = Guid.NewGuid().ToString() + ".png"; 
                string path= @"\\ddr-oaweb01np\c$\dev\wi\upload\" +guid;
               // path = @"\\ensco.ws\Apps\wims\prod\pictures\" + guid;
                path = @"c:\RsopUpload\" + guid;
                string name = fi.Name;
                string []arr = name.Split(new string[] { "__" }, StringSplitOptions.None);
                if(arr.Length <3)
                    continue;
                string jobTitle = arr[1];
                if ( !isAttachment) {
                    int seq;
                    if (!int.TryParse(arr[2].ToLower().Replace(".png", ""), out seq))
                        continue;
                    sql += " exec utl_updateRowanPhoto '" + guid + "', '" + jobTitle + "', " + seq.ToString()+", '"+ arr[0] + "'";
                }else {
                    if(!arr[2].ToLower().Contains("attachment"))
                        continue; 
                    sql += " exec utl_updateRowanAttachment '" + guid + "', '" + jobTitle + "', '" + arr[2]+"', '" + arr[0] + "'";
                }
                fi.CopyTo(path);

                System.Drawing.Image.GetThumbnailImageAbort myCallBack = new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback);
                string s = path; 
                Bitmap myBitmap = new Bitmap(fi.FullName);
                System.Drawing.Image myThumbnail = myBitmap.GetThumbnailImage(50, 50, myCallBack, IntPtr.Zero);
                // s = Server.MapPath("Upload/" + 
                s=s.Replace(".png", "thumb.png");
                myThumbnail.Save(s);
                myThumbnail.Dispose();
                myBitmap.Dispose();
            }
            string con = "user id=WIMS;password=oasql02npwims;Data Source=DDr-OASQL02np;  Initial Catalog=WorkInstruction"; 
            if (this.checkBox4.Checked)
                 con = "user id=kpi_read;password=KPIiadc123;Data Source=DDc-OASQL02;  Initial Catalog=WorkInstruction";
            if (sql != "") {
                this.da = new DataAccessor(con);
                this.da.GetDataSet(sql);
            }
            MessageBox.Show("Done"); 
        }
        public bool ThumbnailCallback() {
            return false;
        }
        private void button4_Click(object sender, EventArgs e) {
            this.UploadPhoto(); 
        }
        private void button5_Click(object sender, EventArgs e) {
            this.UploadPhoto(true); 
        }

        private void button6_Click(object sender, EventArgs e) {
            this.SyncFile(); 
        }
        void SyncFile() {
            foreach(var item in this.listBox1.SelectedItems) {
                string rig = item.ToString();
                if(rig.Trim() == "")
                    continue;
                this.SqlStringBuilder.Clear();
                Thread t = new Thread(new ParameterizedThreadStart( CopyDir)) ;
                t.Start(new RigInfo(rig)); 
                //await this.CopyDir(rig);
            }
        }
        void LogSql(string rig ) {
            //this.textBox1.Text = this.textBox1.Text + this.SqlStringBuilder.ToString();
            //return; 
            string con = "user id=IRMA;password=oasql02npirma;Data Source=ddr-oasql02np;  Initial Catalog=irma_uat";
            this.da = new DataAccessor(con);
            this.SqlStringBuilder.Append(" exec usp_sendEmail 'frank.wang@valaris.com', '" + rig + "', 'done' "); 
            this.da.GetDataSet(this.SqlStringBuilder.ToString());
        }
        void  CopyDir( object obj ) {
            RigInfo rigInfo = obj as RigInfo;
            string rig = rigInfo.Rig;
            DirectoryInfo di = rigInfo.Di; 
            string dir = @"c:\aa";
            if (di == null) {
                di = new DirectoryInfo(dir);
            }

            string[] arr = new string[] { ".htm", ".txt", ".js", ".css",  ".aspx", ".cs" }; 
            foreach(FileInfo fi in di.GetFiles()) {
                string name = fi.FullName;
                if( !arr.Contains(fi.Extension.ToLower()))
                    continue;
                CopyFile(rig, dir, fi.FullName);  
            }
            foreach(DirectoryInfo di1 in di.GetDirectories()) {
                 this.CopyDir( new RigInfo( rig, di1)); 
            }
            if(rigInfo.Di == null) {
                this.LogSql(rig);
                //this.textBox1.Text = this.textBox1.Text + Environment.NewLine + rig;
                this.SetText(rig); 
            }
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
        void  CopyFile(string rig, string dir, string fullName  ) {
            string name = fullName.Replace(dir, "");
            string s = " insert tbl_error1 select getdate(), '" + rig + "', '" + name + "', ";
            try {
                string path = @"\\" + rig.Split('-')[0] + @"-web01\c$\websites\irma\" + name;
                File.Copy(fullName, path, true);
                this.Log(s+"1");
                if (fullName.ToLower().Contains("popupuser.htm".ToLower()))
                    this.Sync_usp_ComSearchUser(rig);
            } catch(Exception ex) {
                this.Log(s+ "'error11 " + ex.Message.Replace("'", "") + "'"); 
            }
        }
        void Log(string s) {
            //this.textBox1.Text = this.textBox1.Text + s;
            //return; 
            this.SqlStringBuilder.Append(s);
        }
        void Sync_usp_ComSearchUser(string rig ) {
           string sql =  File.ReadAllText(Root + "sql.txt", Encoding.UTF8);
            this.DoSql(rig, sql);
            this.Log(" insert tbl_error1 select getdate(), '" + rig + "', 'usp_ComSearchUser', 1 ");
        }
    }
    class RigInfo
    {
       public  string Rig;
        public DirectoryInfo Di; 
        public RigInfo(string rig, DirectoryInfo di) {
            this.Rig = rig;
            this.Di = di; 
        }
        public RigInfo(string rig) {
            this.Rig = rig;
        }
    }
}

