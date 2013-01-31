#region License

/*

Copyright (c) 2009 - 2011 Fatjon Sakiqi

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

*/

#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Cloo;
using Clootils.Properties;

namespace Clootils
{
    public partial class MainForm : Form
    {
        ComputePlatform platform;
        IList<ComputeDevice> devices;
        IList<IExample> exampleList;

        public MainForm()
        {
            InitializeComponent();
            InitializeSettings();

            textBoxLog.Font = new Font(FontFamily.GenericMonospace, 10);
            textBoxLog.Text =
@"[Platform] - Select the OpenCL platform that should be used to run the examples.
[Devices] - Select the OpenCL devices that should be used to run the examples.
[Run] - Runs the selected examples.
[Copy Log] - Copies the content of this box.";

            devices = new List<ComputeDevice>();

            checkedListDevices.CheckOnClick = true;
            checkedListDevices.ItemCheck += new ItemCheckEventHandler(checkedListDevices_ItemCheck);

            comboBoxPlatform.SelectedIndexChanged += new EventHandler(comboBoxPlatform_SelectedIndexChanged);

            // Populate OpenCL Platform ComboBox
            object[] availablePlatforms = new object[ComputePlatform.Platforms.Count];
            for (int i = 0; i < availablePlatforms.Length; i++)
                availablePlatforms[i] = ComputePlatform.Platforms[i].Name;
            comboBoxPlatform.Items.AddRange(availablePlatforms);
            comboBoxPlatform.SelectedIndex = 0;            

            // Set up the example list
            exampleList = new List<IExample>();
            exampleList.Add(new Example());
            exampleList.Add(new InfoExample());
            exampleList.Add(new MappingExample());
            exampleList.Add(new ProgramExample());
            exampleList.Add(new MultipleKernelsExample());
            exampleList.Add(new VectorAddExample());
            exampleList.Add(new CL11Example());

            checkedListExamples.CheckOnClick = true;
            foreach (IExample example in exampleList)
                checkedListExamples.Items.Add(example.Name, true);
        }

        void checkedListDevices_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
                devices.Add(platform.Devices[e.Index]);
            else
                devices.Remove(platform.Devices[e.Index]);
        }

        private void InitializeSettings()
        {
            if (Settings.Default.EditorFont == null)
                Settings.Default.EditorFont = new Font(FontFamily.GenericMonospace, 10);

            if (Settings.Default.LogFont == null)
                Settings.Default.LogFont = new Font(FontFamily.GenericMonospace, 10);

            Settings.Default.Save();
        }

        private string[] ParseLines(string text)
        {
            List<string> lineList = new List<string>();
            StringReader reader = new StringReader(text);
            string line = reader.ReadLine();
            while (line != null)
            {
                lineList.Add(line);
                line = reader.ReadLine();
            }
            return lineList.ToArray();
        }

        private void buttonCopyLog_Click(object sender, EventArgs e)
        {
            if (textBoxLog.Text.Length > 0)
            {
                Clipboard.Clear();
                Clipboard.SetText(textBoxLog.Text);
            }
        }

        private void buttonRunAll_Click(object sender, EventArgs e)
        {
            if (devices.Count == 0)
            {
                MessageBox.Show("No OpenCL device selected!\n\nSelect one or more devices from the list to continue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StringBuilder output = new StringBuilder();
            StringWriter log = new StringWriter(output);

            ComputeContextPropertyList properties = new ComputeContextPropertyList(platform);
            ComputeContext context = new ComputeContext(devices, properties, null, IntPtr.Zero);

            for (int i = 0; i < exampleList.Count; i++)
            {
                if (checkedListExamples.GetItemChecked(i))
                {
                    log.WriteLine("--------------------------------------------------------------------------------");
                    log.WriteLine("Running \"" + exampleList[i].Name + "\"...");
                    log.WriteLine();
                    exampleList[i].Run(context, log);
                    log.WriteLine();
                    log.WriteLine("\"" + exampleList[i].Name + "\" finished.");
                    log.WriteLine("--------------------------------------------------------------------------------");
                    log.Flush();
                }
            }

            log.Close();

            textBoxLog.Lines = ParseLines(output.ToString());
        }

        void comboBoxPlatform_SelectedIndexChanged(object sender, EventArgs e)
        {
            devices.Clear();
            platform = ComputePlatform.Platforms[comboBoxPlatform.SelectedIndex];
            object[] availableDevices = new object[platform.Devices.Count];
            for (int i = 0; i < availableDevices.Length; i++)
                availableDevices[i] = platform.Devices[i].Name;
            checkedListDevices.Items.Clear();
            checkedListDevices.Items.AddRange(availableDevices);
            checkedListDevices.SetItemChecked(0, true);
        }
    }
}