using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.System;
using BlockPuzzleGameToolkit.Scripts.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class ClassicModeHandler : BaseModeHandler
    {
        public Image rhombusImage;

        protected override void LoadScores()
        {
            var persistedBestScore = GetPersistedBestScore();
            bestScore = persistedBestScore;

            // Load current score from game state using the proper mode-specific loading
            var state = GameState.Load(EGameMode.Classic) as ClassicGameState;
            if (state != null)
            {
                score = state.score;
                bestScore = Mathf.Max(persistedBestScore, state.bestScore);
                scoreText.text = score.ToString();
            }
            else
            {
                score = 0;
                scoreText.text = "0";
            }

            bestScoreText.text = bestScore.ToString();
        }

        protected override void SaveGameState()
        {
            var fieldManager = _levelManager.GetFieldManager();
            if (fieldManager != null)
            {
                var state = new ClassicGameState
                {
                    score = score,
                    bestScore = Mathf.Max(bestScore, GetPersistedBestScore()),
                    gameMode = EGameMode.Classic,
                    gameStatus = EventManager.GameStatus
                };
                GameState.Save(state, fieldManager);
            }
        }

        protected override void DeleteGameState()
        {
            GameState.Delete(EGameMode.Classic);
        }

        public override void OnLose()
        {
            bestScore = Mathf.Max(bestScore, GetPersistedBestScore());
            if (score > bestScore)
            {
                ResourceManager.instance.GetResource("Score").Set(score);
                bestScore = score;
            }

            base.OnLose();
        }

        private int GetPersistedBestScore()
        {
            var scoreResource = ResourceManager.instance.GetResource("Score");
            return scoreResource != null ? scoreResource.GetValue() : 0;
        }
    }
}
