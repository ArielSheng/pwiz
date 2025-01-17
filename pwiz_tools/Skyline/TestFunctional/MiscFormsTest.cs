﻿/*
 * Original author: Don Marsh <donmarsh .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2013 University of Washington - Seattle, WA
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pwiz.Skyline;
using pwiz.Skyline.Alerts;
using pwiz.Skyline.Controls.Graphs;
using pwiz.Skyline.Util;
using pwiz.SkylineTestUtil;

namespace pwiz.SkylineTestFunctional
{
    [TestClass]
    public class MiscFormsTest : AbstractFunctionalTest
    {
        [TestMethod, NoParallelTesting(TestExclusionReason.SHARED_DIRECTORY_WRITE)] // No parallel testing because SkylineException.txt is written to Skyline.exe directory by design - and that directory is shared by all workers
        public void TestMiscForms()
        {
            RunFunctionalTest();
        }

        protected override void DoTest()
        {
            // Exercise MultiButtonMsgDlg's use of standard MessageBoxButtons button sets
            foreach (var buttonSet in (MessageBoxButtons[])Enum.GetValues(typeof(MessageBoxButtons)))
            {
                DialogResult result = DialogResult.Ignore;
                RunDlg<MultiButtonMsgDlg>( () =>
                        result = MultiButtonMsgDlg.Show(Program.MainWindow, buttonSet.ToString(), buttonSet),
                    d => d.AcceptButton.PerformClick());
                var expected = DialogResult.None;
                var newDefault = DialogResult.None; // Used in next step, setting the default button
                switch (buttonSet)
                {
                    case MessageBoxButtons.OK:
                        expected = DialogResult.OK;
                        newDefault = expected;
                        break;
                    case MessageBoxButtons.AbortRetryIgnore:
                        expected = DialogResult.Abort;
                        newDefault = DialogResult.Ignore;
                        break;
                    case MessageBoxButtons.OKCancel:
                        expected = DialogResult.OK;
                        newDefault = DialogResult.Cancel;
                        break;
                    case MessageBoxButtons.RetryCancel:
                        expected = DialogResult.Retry;
                        newDefault = DialogResult.Cancel;
                        break;
                    case MessageBoxButtons.YesNo:
                        expected = DialogResult.Yes;
                        newDefault = DialogResult.No;
                        break;
                    case MessageBoxButtons.YesNoCancel:
                        expected = DialogResult.Yes;
                        newDefault = DialogResult.No;
                        break;
                }
                AssertEx.AreEqual(expected, result);

                // Now test setting the default response
                RunDlg<MultiButtonMsgDlg>(() =>
                        result = MultiButtonMsgDlg.Show(Program.MainWindow, buttonSet.ToString(), buttonSet, newDefault),
                    d => d.AcceptButton.PerformClick());
                AssertEx.AreEqual(newDefault, result);

            }

            // Show About dialog.
            RunDlg<AboutDlg>(() =>
                {
                    using (var about = new AboutDlg())
                    {
                        about.ShowDialog(Program.MainWindow);
                    }
                },
                a => a.Close());

            // Show Alert link dialog.
            RunDlg<AlertLinkDlg>(
                () => WebHelpers.ShowLinkFailure(Program.MainWindow, "http://skyline.maccosslab.org"),
                d => d.Close());

            // Show shutdown report dialog
            Assert.IsFalse(ReportShutdownDlg.HadUnexpectedShutdown(true));
            try
            {
                throw new IOException("Something to report");
            }
            catch (Exception x)
            {
                ReportShutdownDlg.SaveExceptionFile(x, true);
            }
            Assert.IsTrue(ReportShutdownDlg.HadUnexpectedShutdown(true));
            RunDlg<ReportShutdownDlg>(() =>
                {
                    using (var reportShutdownDlg = new ReportShutdownDlg())
                    {
                        reportShutdownDlg.ShowDialog(SkylineWindow);
                    }
                },
                d => d.Close());
            Assert.IsFalse(ReportShutdownDlg.HadUnexpectedShutdown(true));

            // Show upgrade dialog
            RunDlg<UpgradeLicenseDlg>(() =>
            {
                using (var dlg = new UpgradeLicenseDlg(Program.LICENSE_VERSION_CURRENT - 1))
                {
                    dlg.ShowDialog(SkylineWindow);
                }
            }, d => d.Close());

            // Show import retry dialog (requires some extra work to avoid blocking the counting)
            var dlgCount = ShowDialog<ImportResultsRetryCountdownDlg>(ShowImportResultsRetryCountdownDlg);
//            Thread.Sleep(20*1000);
            OkDialog(dlgCount, dlgCount.Cancel);
        }

        private void ShowImportResultsRetryCountdownDlg()
        {
            using (var dlg = new ImportResultsRetryCountdownDlg(20, () => { }, () => { }))
            {
                dlg.ShowDialog(SkylineWindow);
            }
        }
    }
}
