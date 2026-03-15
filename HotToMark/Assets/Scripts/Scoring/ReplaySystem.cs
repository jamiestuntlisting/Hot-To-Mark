using UnityEngine;
using System.Collections.Generic;
using HotToMark.Core;

namespace HotToMark.Scoring
{
    /// <summary>
    /// F-5: Replay System — records input/position snapshots during a take,
    /// then plays them back so the player can watch their performance.
    /// </summary>
    public class ReplaySystem : MonoBehaviour
    {
        public bool isRecording;
        public bool isPlaying;
        public float playbackSpeed = 1f;

        private List<ReplayFrame> frames = new List<ReplayFrame>();
        private int playbackIndex;
        private float playbackTimer;
        private float recordInterval = 0.05f; // 20 fps recording
        private float recordTimer;

        private GameState state;

        void Update()
        {
            if (state == null && GameManager.Instance != null)
                state = GameManager.Instance.state;
            if (state == null) return;

            if (isRecording)
                RecordUpdate();
            else if (isPlaying)
                PlaybackUpdate();
        }

        /// <summary>
        /// Start recording a new take. Call at game start.
        /// </summary>
        public void StartRecording()
        {
            frames.Clear();
            isRecording = true;
            isPlaying = false;
            recordTimer = 0;
            playbackIndex = 0;
        }

        /// <summary>
        /// Stop recording. Call when take ends.
        /// </summary>
        public void StopRecording()
        {
            isRecording = false;
        }

        /// <summary>
        /// Start playback of the recorded take.
        /// </summary>
        public void StartPlayback()
        {
            if (frames.Count == 0) return;
            isPlaying = true;
            isRecording = false;
            playbackIndex = 0;
            playbackTimer = 0;
        }

        /// <summary>
        /// Stop playback.
        /// </summary>
        public void StopPlayback()
        {
            isPlaying = false;
        }

        public bool HasRecording => frames.Count > 0;
        public float RecordingDuration => frames.Count > 0 ? frames[frames.Count - 1].time : 0;
        public float PlaybackTime => isPlaying && playbackIndex < frames.Count
            ? frames[playbackIndex].time : 0;
        public float PlaybackProgress => frames.Count > 0
            ? (float)playbackIndex / frames.Count : 0;

        private void RecordUpdate()
        {
            recordTimer += Time.deltaTime;
            if (recordTimer < recordInterval) return;
            recordTimer -= recordInterval;

            frames.Add(new ReplayFrame
            {
                time = Time.time - state.takeStartTime,
                posX = state.posX,
                posY = state.posY,
                speed = state.speed,
                steering = state.steering,
                throttle = state.throttle,
                brake = state.brake,
                gear = state.gear,
                phase = state.phase,
                wheelAngle = state.wheelAngle
            });
        }

        private void PlaybackUpdate()
        {
            if (playbackIndex >= frames.Count)
            {
                StopPlayback();
                return;
            }

            playbackTimer += Time.deltaTime * playbackSpeed;

            // Advance to the correct frame
            while (playbackIndex < frames.Count - 1 &&
                   frames[playbackIndex + 1].time <= playbackTimer)
            {
                playbackIndex++;
            }

            var frame = frames[playbackIndex];

            // Apply frame data to car position (visual only)
            if (GameManager.Instance != null && GameManager.Instance.carController != null)
            {
                var car = GameManager.Instance.carController;
                var worldPos = Vehicle.CarController.FeetToWorldPosition(frame.posY, frame.posX);
                car.transform.position = worldPos;
            }

            // Update state for HUD display
            if (state != null)
            {
                state.posX = frame.posX;
                state.posY = frame.posY;
                state.speed = frame.speed;
                state.steering = frame.steering;
                state.wheelAngle = frame.wheelAngle;
            }
        }

        /// <summary>
        /// Seek to a specific progress (0-1).
        /// </summary>
        public void SeekTo(float progress)
        {
            if (frames.Count == 0) return;
            playbackIndex = Mathf.Clamp(Mathf.RoundToInt(progress * (frames.Count - 1)),
                0, frames.Count - 1);
            playbackTimer = frames[playbackIndex].time;
        }
    }

    [System.Serializable]
    public struct ReplayFrame
    {
        public float time;
        public float posX;
        public float posY;
        public float speed;
        public float steering;
        public float throttle;
        public float brake;
        public Gear gear;
        public GamePhase phase;
        public float wheelAngle;
    }
}
