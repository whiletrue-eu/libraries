﻿using NUnit.Framework;

namespace WhileTrue.Classes.Wpf._Unittest_Debug_
{
    [TestFixture]
    public class CrossThreadCollectionWrapperTest
    {
        [Test, Ignore("Manual test")]
        public void ManualTest()
        {
            new CrossThreadCollectionWrapperWindow().ShowDialog();
        }
    }
}
