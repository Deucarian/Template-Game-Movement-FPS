using System;
using System.Collections.Generic;
using Deucarian.Common;
using Deucarian.Combat;
using Deucarian.RunUpgrades;
using Deucarian.TemplateGameMovementFps.Actors;
using Deucarian.TemplateGameMovementFps.Combat;
using Deucarian.TemplateGameMovementFps.Movement;
using Deucarian.TemplateGameMovementFps.Progression;
using Deucarian.TemplateGameMovementFps.Run;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deucarian.TemplateGameMovementFps
{
    internal sealed class MovementFpsHudPresenter
    {
        private GUIStyle _hudTitleStyle;
        private GUIStyle _hudLabelStyle;
        private GUIStyle _hudSmallStyle;

        private void EnsureHudStyles()
        {
            if (_hudTitleStyle != null)
            {
                return;
            }

            _hudTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.84f, 0.94f, 1f) }
            };
            _hudLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = Color.white }
            };
            _hudSmallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                wordWrap = true,
                normal = { textColor = new Color(0.78f, 0.88f, 0.95f) }
            };
        }

        private void DrawHudBar(string label, float value, Color fill)
        {
            Rect rect = GUILayoutUtility.GetRect(392f, 18f);
            GUI.Box(rect, GUIContent.none);
            Rect fillRect = new Rect(rect.x + 2f, rect.y + 2f, Mathf.Max(0f, rect.width - 4f) * Mathf.Clamp01(value), rect.height - 4f);
            Color oldColor = GUI.color;
            GUI.color = fill;
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = oldColor;
            GUI.Label(rect, label + " " + Mathf.RoundToInt(Mathf.Clamp01(value) * 100f).ToString() + "%", _hudSmallStyle);
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }

        internal void Draw(MovementFpsHudSnapshot snapshot)
        {
            EnsureHudStyles();
            GUILayout.BeginArea(new Rect(16f, 16f, 430f, 276f), GUI.skin.box);
            GUILayout.Label("Movement FPS Template", _hudTitleStyle);
            DrawHudBar("Health", snapshot.MaximumHealth <= 0d ? 0f : (float)(snapshot.CurrentHealth / snapshot.MaximumHealth), new Color(0.9f, 0.24f, 0.22f));
            DrawHudBar("XP", snapshot.CurrentExperience / (float)snapshot.RequiredExperience, new Color(0.24f, 0.82f, 1f));
            DrawHudBar("Run", Mathf.Clamp01(snapshot.Elapsed / Mathf.Max(1f, snapshot.VictoryTime)), new Color(0.78f, 0.48f, 1f));
            GUILayout.Label($"Level {snapshot.Level}   Time {FormatTime(snapshot.Elapsed)}   State {snapshot.RunState}", _hudLabelStyle);
            GUILayout.Label($"Enemies {snapshot.EnemyCount}/{snapshot.Spawn.MaxAlive}   Wave batch {snapshot.Spawn.BatchSize}", _hudLabelStyle);
            MovementFpsRunSummary summary = snapshot.Summary;
            GUILayout.Label($"Kills {summary.KillCount}   XP gained {summary.ExperienceGained}   Upgrades {summary.UpgradesChosen.Count}", _hudLabelStyle);
            GUILayout.Label($"Move {snapshot.MovementState}   Speed {snapshot.Speed:0.0}", _hudLabelStyle);
            GUILayout.Label($"Loadout: {snapshot.GunCount} guns / {snapshot.PowerCount} powers", _hudSmallStyle);
            if (snapshot.HasGun)
            {

                GUILayout.Label($"{snapshot.GunName}  Ammo {snapshot.Ammo}/{snapshot.MagazineSize}{(snapshot.Reloading ? "  Reloading" : string.Empty)}", _hudLabelStyle);
            }

            if (snapshot.DraftOpen)
            {
                GUILayout.Label("Choose upgrade: 1 / 2 / 3", _hudLabelStyle);
                for (int index = 0; index < snapshot.DraftChoices.Count; index++)
                {
                    GUILayout.Label($"{index + 1}. {snapshot.DraftChoices[index]}", _hudSmallStyle);
                }
            }
            else if (snapshot.Defeated)
            {
                GUILayout.Label("Defeated - press R to restart", _hudLabelStyle);
                GUILayout.Label($"Summary: survived {summary.ElapsedSeconds:0}s, kills {summary.KillCount}, XP {summary.ExperienceGained}", _hudSmallStyle);
            }
            else if (snapshot.Victory)
            {
                GUILayout.Label("Victory - press R to restart", _hudLabelStyle);
                GUILayout.Label($"Summary: cleared in {summary.ElapsedSeconds:0}s, kills {summary.KillCount}, rewards {summary.Rewards.Count}", _hudSmallStyle);
            }
            else
            {
                GUILayout.Label("WASD move, Shift sprint, Ctrl/C slide, Space jump, mouse fire/look, Q swap guns", _hudSmallStyle);
            }

            GUILayout.EndArea();
        }
    }
}
