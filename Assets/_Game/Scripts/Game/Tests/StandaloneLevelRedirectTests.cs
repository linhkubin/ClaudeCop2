using NUnit.Framework;

namespace ClaudeCop.Game.Tests
{
    /// <summary>L-CHAIN-ENTRY: Play thang scene level le -> chuyen sang chuoi tu level do; qua Title / dang o chuoi / tat co -> khong chuyen.</summary>
    public class StandaloneLevelRedirectTests
    {
        [Test]
        public void Redirects_WhenPlayedDirectly_FromStandaloneLevel()
        {
            Assert.IsTrue(StandaloneLevelRedirect.ShouldRedirect(true, false, "Level_02", 1));
            Assert.IsTrue(StandaloneLevelRedirect.ShouldRedirect(true, false, "Level_01", 0));
        }

        [Test]
        public void NoRedirect_AfterTitle_InChain_OrDisabled()
        {
            Assert.IsFalse(StandaloneLevelRedirect.ShouldRedirect(true, true, "Level_02", 1), "da qua Title: Title nap thang Level_Chain");
            Assert.IsFalse(StandaloneLevelRedirect.ShouldRedirect(true, false, StandaloneLevelRedirect.ChainSceneName, 1), "dang o chuoi: khong nap lai chinh no");
            Assert.IsFalse(StandaloneLevelRedirect.ShouldRedirect(false, false, "Level_02", 1), "tat co: choi rieng level");
            Assert.IsFalse(StandaloneLevelRedirect.ShouldRedirect(true, false, "Level_02", -1));
        }
    }
}
