using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.Face;

namespace DemoApp
{
    public partial class Form1 : Form
    {
        const string personGroupId = "family3";

        Color[] colors = { Color.White, Color.Red, Color.Green, Color.Blue, Color.Aqua, Color.Yellow };
        Pen[] pens;

        IFaceClient faceClient = new FaceClient(
           new ApiKeyServiceClientCredentials("1a0dbec9939347118f891fa5ab6fcdf2"),  // 1a0dbec9939347118f891fa5ab6fcdf2   392f60c535434c0dba36b2c1b7753e8b
           new System.Net.Http.DelegatingHandler[] { })
        { Endpoint = "https://westcentralus.api.cognitive.microsoft.com/" };

        public Form1()
        {
            InitializeComponent();

            pens = colors.Select(t => new Pen(t, 3)).ToArray();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (string imagePath in Directory.GetFiles("TestPictures", "*.jpg"))
            {
                FileInfo fi = new FileInfo(imagePath);
                listBox1.Items.Add(fi.Name);
            }
        }

        Image bm;
        string fn;

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            fn = null;
            if (listBox1.SelectedItem == null)
                return;
            fn = Path.Combine("TestPictures", listBox1.SelectedItem.ToString());
            bm = Bitmap.FromFile(fn);
            pictureBox1.Image = bm;
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            listBox2.Items.Add(string.Format("{0}-{1}", "Unknown", colors[0]));
            ListBox1_SelectedIndexChanged(null, null);
            if (fn == null)
                return;

            using (Graphics gr = Graphics.FromImage(bm))
            {
                using (Stream s = File.OpenRead(fn))
                {
                    var faces = await faceClient.Face.DetectWithStreamAsync(s);
                    var faceIds = faces.Select(face => face.FaceId.Value).ToArray();

                    var results = await faceClient.Face.IdentifyAsync(faceIds, personGroupId);
                    int cn = 1;
                    foreach (var identifyResult in results)
                    {
                        var rr = faces.Where(t => t.FaceId == identifyResult.FaceId).Select(t => t.FaceRectangle).ToArray();
                        if (identifyResult.Candidates.Count == 0)
                        {
                            foreach (var r in rr)
                            {
                                gr.DrawRectangle(pens[0], r.Left, r.Top, r.Width, r.Height);
                            }
                        }
                        else
                        {
                            // Get top 1 among all candidates returned
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var person = await faceClient.PersonGroupPerson.GetAsync(personGroupId, candidateId);

                            listBox2.Items.Add(string.Format("{0}-{1}",person.Name,colors[cn]));

                            foreach (var r in rr)
                            {
                                gr.DrawRectangle(pens[cn], r.Left, r.Top, r.Width, r.Height);
                            }

                            cn++;
                        }
                    }
                }
            }

            pictureBox1.Refresh();
        }
    }
}
