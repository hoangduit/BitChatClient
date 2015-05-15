﻿/*
Technitium Bit Chat
Copyright (C) 2015  Shreyas Zare (shreyas@technitium.com)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using BitChatClient;
using TechnitiumLibrary.Net.BitTorrent;
using TechnitiumLibrary.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BitChatApp
{
    public partial class frmRegister : Form
    {
        #region variables

        BitChatProfile _profile;
        string _profileFilePath;
        string _localAppData;

        #endregion

        #region constructor

        public frmRegister(string localAppData)
        {
            _localAppData = localAppData;

            InitializeComponent();

            this.Width = 700 + 50;
            this.Height = 445;

            pnlRegister.Left = 12 + 56;
            pnlRegister.Top = 12;

            pnlMessages.Left = 12 + 56;
            pnlMessages.Top = 12;

            pnlDownloadCert.Left = 12 + 56;
            pnlDownloadCert.Top = 12;
        }

        public frmRegister(BitChatProfile profile, string profileFilePath)
        {
            _profile = profile;
            _profileFilePath = profileFilePath;

            InitializeComponent();

            this.Width = 700 + 56;
            this.Height = 450;

            pnlRegister.Left = 12 + 56;
            pnlRegister.Top = 12;

            pnlMessages.Left = 12 + 56;
            pnlMessages.Top = 12;

            pnlDownloadCert.Left = 12 + 56;
            pnlDownloadCert.Top = 12;

            pnlRegister.Visible = false;
            pnlDownloadCert.Visible = true;
        }

        #endregion

        #region private

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to close this window?", "Close Window?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Ignore;
                this.Close();
            }
        }

        private void chkAccept_CheckedChanged(object sender, EventArgs e)
        {
            btnRegister.Enabled = chkAccept.Checked;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://go.technitium.com/?id=3");
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string name = null;
            MailAddress emailAddress = null;
            Uri website = null;
            string phoneNumber = null;
            string streetAddress = null;
            string city = null;
            string state = null;
            string country = null;
            string postalCode = null;

            #region validate form

            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Please enter a valid name.", "Name Missing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtEmail.Text))
            {
                MessageBox.Show("Please enter a valid email address.", "Email Address Missing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtEmail.Focus();
                return;
            }
            else
            {
                try
                {
                    emailAddress = new MailAddress(txtEmail.Text);
                }
                catch
                {
                    MessageBox.Show("Please enter a valid email address.", "Invalid Email Address!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    txtEmail.Focus();
                    return;
                }
            }

            if (!string.IsNullOrEmpty(txtWebsite.Text))
            {
                try
                {
                    website = new Uri(txtWebsite.Text);
                }
                catch
                {
                    MessageBox.Show("Please enter a valid web address. Example: http://example.com", "Invalid Web Address!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    txtWebsite.Focus();
                    return;
                }
            }

            if (string.IsNullOrEmpty(txtCountry.Text))
            {
                MessageBox.Show("Please select a valid country name.", "Country Name Missing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtCountry.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtProfilePassword.Text))
            {
                MessageBox.Show("Please enter a profile password.", "Profile Password Missing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtProfilePassword.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtConfirmPassword.Text))
            {
                MessageBox.Show("Please confirm profile password.", "Confirm Password Missing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtConfirmPassword.Focus();
                return;
            }

            if (txtConfirmPassword.Text != txtProfilePassword.Text)
            {
                txtProfilePassword.Text = "";
                txtConfirmPassword.Text = "";
                MessageBox.Show("Profile password doesn't match with confirm profile password. Please enter both passwords again.", "Password Mismatch!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtProfilePassword.Focus();
                return;
            }

            #endregion

            if (!string.IsNullOrEmpty(txtName.Text))
                name = txtName.Text;

            if (!string.IsNullOrEmpty(txtPhone.Text))
                phoneNumber = txtPhone.Text;

            if (!string.IsNullOrEmpty(txtStreetAddress.Text))
                streetAddress = txtStreetAddress.Text;

            if (!string.IsNullOrEmpty(txtCity.Text))
                city = txtCity.Text;

            if (!string.IsNullOrEmpty(txtState.Text))
                state = txtState.Text;

            if (!string.IsNullOrEmpty(txtCountry.Text))
                country = txtCountry.Text;

            if (!string.IsNullOrEmpty(txtPostalCode.Text))
                postalCode = txtPostalCode.Text;


            CertificateProfile profile = new CertificateProfile(name, CertificateProfileType.Individual, emailAddress, website, phoneNumber, streetAddress, city, state, country, postalCode);

            pnlRegister.Visible = false;
            lblPanelTitle.Text = "Registering...";
            lblPanelMessage.Text = "Please wait while we generate your profile private key and register your profile certificate.\r\n\r\nRegistering on " + Program.SIGNUP_URI.Host + " ...";
            pnlMessages.Visible = true;

            Action<CertificateProfile> d = new Action<CertificateProfile>(RegisterAsync);
            d.BeginInvoke(profile, null, null);
        }

        private void btnDownloadAndStart_Click(object sender, EventArgs e)
        {
            try
            {
                _profile.LocalCertificateStore.Certificate = Registration.GetSignedCertificate(Program.SIGNUP_URI, _profile.LocalCertificateStore);

                using (FileStream fS = new FileStream(_profileFilePath, FileMode.Create, FileAccess.Write))
                {
                    _profile.WriteTo(fS);
                }

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error orrured while downloading profile certificate:\r\n\r\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RegisterAsync(CertificateProfile profile)
        {
            try
            {
                //register
                AsymmetricCryptoKey privateKey = new AsymmetricCryptoKey(AsymmetricEncryptionAlgorithm.RSA, 4096);
                Certificate selfSignedCert = new Certificate(CertificateType.RootCA, "", profile, CertificateCapability.SignCACertificate, DateTime.UtcNow, DateTime.UtcNow, AsymmetricEncryptionAlgorithm.RSA, privateKey.GetPublicKey());
                selfSignedCert.SelfSign("SHA256", privateKey, null);

                Registration.Register(Program.SIGNUP_URI, selfSignedCert);

                _profile = new BitChatProfile(null, new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0), GetDownloadsPath(), BitChatProfile.DefaultTrackerURIs);
                _profile.LocalCertificateStore = new CertificateStore(selfSignedCert, privateKey);
                _profile.SetPassword(SymmetricEncryptionAlgorithm.Rijndael, 256, txtProfilePassword.Text);

                _profileFilePath = Path.Combine(_localAppData, _profile.LocalCertificateStore.Certificate.IssuedTo.Name + ".profile");

                using (FileStream fS = new FileStream(_profileFilePath, FileMode.Create, FileAccess.Write))
                {
                    _profile.WriteTo(fS);
                }

                this.Invoke(new Action<object>(RegistrationSuccess), new object[] { null });
            }
            catch (Exception ex)
            {
                this.Invoke(new Action<object>(RegistrationFail), new object[] { ex.Message });
            }
        }

        private void RegistrationSuccess(object state)
        {
            pnlMessages.Visible = false;
            pnlRegister.Visible = false;
            pnlDownloadCert.Visible = true;
        }

        private void RegistrationFail(object state)
        {
            MessageBox.Show("Error orrured while registering for profile certificate:\r\n\r\n" + (string)state, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            pnlMessages.Visible = false;
            pnlRegister.Visible = true;
        }

        #region download folder

        private static string GetDownloadsPath()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                IntPtr pathPtr;
                int hr = SHGetKnownFolderPath(ref FolderDownloads, 0, IntPtr.Zero, out pathPtr);
                if (hr == 0)
                {
                    string path = Marshal.PtrToStringUni(pathPtr);
                    Marshal.FreeCoTaskMem(pathPtr);
                    return path;
                }
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }

        private static Guid FolderDownloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);

        #endregion

        #endregion

        #region properties

        public BitChatProfile Profile
        { get { return _profile; } }

        public string ProfileFilePath
        { get { return _profileFilePath; } }

        #endregion
    }
}