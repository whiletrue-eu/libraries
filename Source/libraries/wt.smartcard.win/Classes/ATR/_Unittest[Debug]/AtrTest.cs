// ReSharper disable InconsistentNaming
// ReSharper disable CSharpWarnings::CS1591
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    [TestFixture]
    public class AtrTest
    {
        [Test]
        public void T0_Should_be_kept_if_T1_is_added_but_t0_was_only_indicated_by_default()
        {
            Atr Atr = new Atr(new byte[] { 0x3F, 0x42, 0x00, 0x21, 0x45 });
            Atr.IndicateProtocol(ProtocolType.T1);
            Assert.That(Atr.Bytes.ToHexString(), Is.EqualTo(new byte[] { 0x3F, 0xC2, 0x00, 0x80, 0x01, 0x21, 0x45, 0x27 }.ToHexString()));
        }


        [Test,Ignore("Manual Test")]
        public void MassTest()
        {
            bool HasErrors = false;

            ATREntry[] ATRs = this.GetATRs();
            foreach (ATREntry AtrEntry in ATRs)
            {
                try
                {
                    new Atr(AtrEntry.Atr);
                }
                catch (Exception Error)
                {
                    HasErrors = true;
                    Debug.WriteLine($"{AtrEntry.Description}\n({AtrEntry.Atr.ToHexString()}):\n{Error.GetType().FullName}:{Error.Message}\n");
                }
            }

            Assert.IsFalse(HasErrors);
        }

        private ATREntry[] GetATRs()
        {
            List<ATREntry> ATRs = new List<ATREntry>();
            TextReader Source = new StringReader(TestResources.ATRs);
            while(Source.Peek() != -1)
            {
                try
                {
                    ATRs.Add(new ATREntry(Source));
                }
                catch
                {
                    // Skip invalid entries
                }
            }
            return ATRs.ToArray();
        }
    }

    internal class ATREntry
    {
        public ATREntry(TextReader source)
        {
            string Line = source.ReadLine();
            List<string>EntryLines = new List<string>();
            while(Line != null && (string.IsNullOrEmpty(Line)==false || EntryLines.Count < 1))
            {
                if (string.IsNullOrEmpty(Line.Trim())==false && Line.Trim().StartsWith("#") == false)
                {
                    EntryLines.Add(Line.Trim());
                }
                else
                {
                    //ignore '#' comments
                }
                Line = source.ReadLine();
            }

            this.Atr = EntryLines[0].ToByteArray();
            this.Description = string.Join("\n",EntryLines.ToArray().GetSubArray(1));
        }

        public string Description { get; set; }
        public byte[] Atr { get; set; }
    }
}
