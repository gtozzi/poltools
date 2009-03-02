﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FiddlerPlugin
{
    public partial class Option : Form
    {
        public Option()
        {
            InitializeComponent();
            cmdtext.Text = SendItemPlugin.Cmd;
            argstext.Text = SendItemPlugin.CmdArg;
            SendOnClick.Checked = SendItemPlugin.OverrideClick;
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            SendItemPlugin.Cmd = cmdtext.Text;
            SendItemPlugin.CmdArg = argstext.Text;
            SendItemPlugin.OverrideClick = SendOnClick.Checked;
            Close();
        }
    }
}