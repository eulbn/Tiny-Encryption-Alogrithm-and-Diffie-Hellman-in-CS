using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
namespace Tea1
{
    public partial class Form1 : Form
    {

        UInt32[] privatekey;
        UInt32[] key;
        public Form1()
        {
            
            InitializeComponent();
            privatekey = new UInt32[4];
            key = new UInt32[4];
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel1.BringToFront();

        }
        private void button2_Click(object sender, EventArgs e)
        {
            panel3.BringToFront();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ImageEncrypt();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            TextEncrypt();
            textBox1.Text = "";
        }
        private void button6_Click(object sender, EventArgs e)
        {
            TextDecrypte();
            pictureBox1.Image = ImageDecrypt();
        }

        

        void KeyExchangeWrite()
        {
            
            UInt32 G = 97;
            UInt32 p = 83;
            

            
            UInt32[] pubilcKey = new UInt32[4];
            string[] pubilcKeyStrings = new string[4];
            for(int i = 0; i < pubilcKey.Length; i++)
            {
                
                pubilcKey[i] = Convert.ToUInt32((Math.Pow(G, privatekey[i])) % p);
                pubilcKeyStrings[i] = pubilcKey[i].ToString();
            }

            File.WriteAllLines("PublicKey.txt", pubilcKeyStrings);
            
        }
        void KeyExchangeRead()
        {
            
            UInt32 G = 97;
            UInt32 p = 83;

            string[] read = File.ReadAllLines("publicKey.txt");
            File.Delete("publicKey.txt");
            UInt32[] pubilcKey = new UInt32[4];
            for (int i = 0; i < read.Length; i++)
            {
                pubilcKey[i] = Convert.ToUInt32(read[i]);
                key[i] = Convert.ToUInt32((Math.Pow(pubilcKey[i], privatekey[i])) % p);

            }

            string s = "Keys:";

            foreach (var x in key)
                s += x.ToString() + " ";

            textBox3.Text = s;

        }

        private void TextEncrypt()
        {
            byte[] temData = Encoding.ASCII.GetBytes(textBox1.Text);


            byte[] data;
            if (temData.Length % 8 != 0)
                data = new byte[temData.Length + (8 - (temData.Length % 8))];
            else
                data = new byte[temData.Length];


            for (int i = 0; i < data.Length; i++)
                if (i < temData.Length)
                    data[i] = temData[i];


            byte[] Encrypted = Encrypt(data);

            using (FileStream fs = File.Create("EncryptedText.bin"))
            {
                fs.Write(Encrypted, 0, Encrypted.Length);
            }
            File.WriteAllText("OffsetText.txt", temData.Length.ToString());
        }

        private void TextDecrypte()
        {
            byte[] Encrypted = File.ReadAllBytes("EncryptedText.bin");
            int temDataLength = Convert.ToInt32(File.ReadAllText("OffsetText.txt"));

            File.Delete("EncryptedText.bin");
            File.Delete("OffsetText.txt");


            byte[] temData = new byte[temDataLength];

            byte[] Decrypted = Decrypt(Encrypted);

            for (int i = 0; i < temData.Length; i++)
                temData[i] = Decrypted[i];


            textBox2.Text = Encoding.ASCII.GetString(temData);
        }
    




        private void ImageEncrypt()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Files|*.jpg;*.jpeg;*.png"; // file types, that will be allowed to upload
            dialog.Multiselect = false; // allow/deny user to upload more than one file at a time



            if (dialog.ShowDialog() == DialogResult.OK) // if user clicked OK
            {

                Image image = Image.FromFile(dialog.FileName);
                byte[] temData = imageToByteArray(image);



                byte[] data;
                if (temData.Length % 8 != 0)
                    data = new byte[temData.Length + (8 - (temData.Length % 8))];
                else
                    data = new byte[temData.Length];
                

                for (int i = 0; i < data.Length; i++)
                    if (i < temData.Length)
                        data[i] = temData[i];


                byte[] Encrypted = Encrypt(data);

                using (FileStream fs = File.Create("EncryptedImage.bin"))
                {
                    fs.Write(Encrypted, 0, Encrypted.Length);
                }
                File.WriteAllText("OffsetImage.txt", temData.Length.ToString());

            }


        }
        private Image ImageDecrypt()
        {


            byte[] Encrypted = File.ReadAllBytes("EncryptedImage.bin");
            int temDataLength = Convert.ToInt32(File.ReadAllText("OffsetImage.txt"));

            File.Delete("EncryptedImage.bin");
            File.Delete("OffsetImage.txt");


            byte[] temData = new byte[temDataLength];

            byte[] Decrypted = Decrypt(Encrypted);


            for (int i = 0; i < temData.Length; i++)
                temData[i] = Decrypted[i];

            Image img = byteArrayToImage(temData);
            return img;
        }



        byte[] Encrypt(byte[] data)
        {
            
            
            int k = 0;

            //UInt32[] key = { 0xa56babcd, 0x00000000, 0xffffffff, 0xabcdef01 };
            for (int i = 0; i < data.Length; i += 8)
            {
                UInt16 tem1 = Convert.ToUInt16((UInt16) data[i] << 8 | data[i + 1]);
                UInt16 tem2 = Convert.ToUInt16((UInt16)data[i + 2] << 8 | data[i + 3]);

                UInt16 tem3 = Convert.ToUInt16((UInt16)data[i + 4] << 8 | data[i + 5]);
                UInt16 tem4 = Convert.ToUInt16((UInt16)data[i + 6] << 8 | data[i + 7]);

                UInt32 temp1 = Convert.ToUInt32((UInt32)tem1 << 16 | tem2);
                UInt32 temp2 = Convert.ToUInt32((UInt32)tem3 << 16 | tem4);

                UInt32[] tem = { temp1, temp2 };


                tem = encrypt(tem, key);


                byte[] bytes = BitConverter.GetBytes(tem[0]);

                Array.Reverse(bytes);
                data[i] = bytes[0];
                data[i + 1] = bytes[1];
                data[i + 2] = bytes[2];
                data[i + 3] = bytes[3];



                bytes = BitConverter.GetBytes(tem[1]);

                Array.Reverse(bytes);
                data[i + 4] = bytes[0];
                data[i + 5] = bytes[1];
                data[i + 6] = bytes[2];
                data[i + 7] = bytes[3];


                k += 2;
            }
            return data;
        }
        byte[] Decrypt(byte[] data)
        {

            int k = 0;

            //UInt32[] key = { 0xa56babcd, 0x00000000, 0xffffffff, 0xabcdef01 };
            for (int i = 0; i < data.Length; i += 8)
            {
                UInt16 tem1 = Convert.ToUInt16(data[i] << 8 | data[i + 1]);
                UInt16 tem2 = Convert.ToUInt16(data[i + 2] << 8 | data[i + 3]);

                UInt16 tem3 = Convert.ToUInt16(data[i + 4] << 8 | data[i + 5]);
                UInt16 tem4 = Convert.ToUInt16(data[i + 6] << 8 | data[i + 7]);

                UInt32 temp1 = Convert.ToUInt32((UInt32)tem1 << 16 | tem2);
                UInt32 temp2 = Convert.ToUInt32((UInt32)tem3 << 16 | tem4);

                UInt32[] tem = { temp1, temp2 };


                tem = decrypt(tem, key);


                byte[] bytes = BitConverter.GetBytes(tem[0]);

                Array.Reverse(bytes);
                data[i] = bytes[0];
                data[i + 1] = bytes[1];
                data[i + 2] = bytes[2];
                data[i + 3] = bytes[3];

                bytes = BitConverter.GetBytes(tem[1]);

                Array.Reverse(bytes);
                data[i + 4] = bytes[0];
                data[i + 5] = bytes[1];
                data[i + 6] = bytes[2];
                data[i + 7] = bytes[3];


                k += 2;
            }
            return data;
        }



        UInt32[] encrypt(UInt32[] v, UInt32[] KEY)// v= data block 2 values of 64 bits, key = of 4 values of 128 bits
        {
            UInt32 v0 = v[0], v1 = v[1], sum = 0, i;
            UInt32 delta = 0x9e3779b9;
            for (i = 0; i < 32; i++)
            {
               
                sum += delta;
                v0 += ((v1 << 4) + KEY[0]) ^ (v1 + sum) ^ ((v1 >> 5) + KEY[1]);// ^ = xor gate
                v1 += ((v0 << 4) + KEY[2]) ^ (v0 + sum) ^ ((v0 >> 5) + KEY[3]);// << bit shift left, .. bit shift right
            }
            v[0] = v0; v[1] = v1;
            return v;
        }

        UInt32[] decrypt(UInt32[] v, UInt32[] KEY)
        {
            UInt32 v0 = v[0], v1 = v[1], sum = 0xC6EF3720, i;
            UInt32 delta = 0x9e3779b9;
            for (i = 0; i < 32; i++)
            {                         // basic cycle start 
                v1 -= ((v0 << 4) + KEY[2]) ^ (v0 + sum) ^ ((v0 >> 5) + KEY[3]);
                v0 -= ((v1 << 4) + KEY[0]) ^ (v1 + sum) ^ ((v1 >> 5) + KEY[1]);
                sum -= delta;
            }
            v[0] = v0; v[1] = v1;
            return v;
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }
        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel2.BringToFront();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            KeyExchangeWrite();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            KeyExchangeRead();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Random rd = new Random();
            for (int i = 0; i < privatekey.Length; i++)
                privatekey[i] = Convert.ToUInt32(rd.Next(3, 7));
            string ss = "Privaet Key:";
            foreach (var x in privatekey)
                ss += x.ToString() + " ";
            textBox4.Text = ss;
        }
    }
}
