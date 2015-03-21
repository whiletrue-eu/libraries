using System;
using System.Windows;
using NUnit.Framework;

namespace WhileTrue.Classes.Wpf._Unittest_Debug_
{
    [TestFixture]
    public class Test
    {
        [Test]
        public void TheTest()
        {
            new Window
                {
                    Content =
                        new DesignDataSampleControl()
                }.ShowDialog();
        }
    }

    public class SampleModel
    {
        public string StringProperty
        {
            get{throw new NotImplementedException();}
        }
        public int IntProperty
        {
            get;
            set;
        }
        public SubModel SubProperty
        {
            get;
            set;
        }
    }

    public class SubModel
    {
        public string SubStringProperty
        {
            get;set;
        }
    }

}