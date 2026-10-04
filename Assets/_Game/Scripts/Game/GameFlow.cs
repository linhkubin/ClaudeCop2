using ClaudeCop.Core;

namespace ClaudeCop.Game
{
    /// <summary>
    /// May trang thai game thuan (test duoc). Hop le: Title->Playing; Playing->RevivePrompt|Win|GameOver;
    /// RevivePrompt->Playing|GameOver; Win/GameOver->Playing (restart/reset).
    /// </summary>
    public sealed class GameFlow
    {
        public GameState State { get; private set; } = GameState.Title;
        public bool ReviveEnabled { get; set; }
        public int RevivesRemaining { get; private set; }

        public void Begin(int revives)
        {
            RevivesRemaining = revives;
            State = GameState.Playing;
        }

        /// <summary>Het mang. Tra state moi (RevivePrompt neu bat revive va con luot, nguoc lai GameOver).</summary>
        public GameState OnOutOfLives()
        {
            if (State != GameState.Playing) return State;
            State = ReviveEnabled && RevivesRemaining > 0 ? GameState.RevivePrompt : GameState.GameOver;
            return State;
        }

        public GameState OnLevelCompleted()
        {
            if (State == GameState.Playing) State = GameState.Win;
            return State;
        }

        /// <summary>Dong y revive: dung 1 luot, ve Playing. false neu khong o RevivePrompt hoac het luot.</summary>
        public bool AcceptRevive()
        {
            if (State != GameState.RevivePrompt || RevivesRemaining <= 0) return false;
            RevivesRemaining--;
            State = GameState.Playing;
            return true;
        }

        public GameState DeclineRevive()
        {
            if (State == GameState.RevivePrompt) State = GameState.GameOver;
            return State;
        }

        /// <summary>Ve man Title tu bat ky state nao (huy luot choi).</summary>
        public GameState ReturnToTitle()
        {
            State = GameState.Title;
            RevivesRemaining = 0;
            return State;
        }

        public bool IsEnded => State == GameState.Win || State == GameState.GameOver;
    }
}
