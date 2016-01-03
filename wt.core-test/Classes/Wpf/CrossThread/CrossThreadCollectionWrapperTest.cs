using NUnit.Framework;

namespace WhileTrue.Classes.Wpf.CrossThread
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
