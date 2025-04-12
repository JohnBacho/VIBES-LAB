import cv2
import csv
from deepface import DeepFace
from collections import defaultdict

def analyze_video_emotions(video_path):
    emotion_stats = defaultdict(lambda: {'count': 0, 'total_confidence': 0.0})
    
    cap = cv2.VideoCapture(video_path)
    if not cap.isOpened():
        print("Error opening video file")
        return

    fps = cap.get(cv2.CAP_PROP_FPS)
    frame_number = 0

    with open('emotion_log.csv', 'w', newline='') as log_file, \
         open('emotion_summary.csv', 'w', newline='') as summary_file:

        log_writer = csv.writer(log_file)
        summary_writer = csv.writer(summary_file)

        log_writer.writerow(['Timestamp (s)', 'Frame', 'Emotion', 'Confidence (%)'])

        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break

            frame_number += 1
            timestamp = frame_number / fps
            rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

            try:
                faces = DeepFace.extract_faces(
                    img_path=rgb_frame,
                    detector_backend='opencv',
                    enforce_detection=False,
                    align=False
                )
            except Exception:
                continue

            for face_obj in faces:
                region = face_obj['facial_area']
                x, y, w, h = region['x'], region['y'], region['w'], region['h']

                # Define mouth region as bottom 50% of the face
                mouth_x_start = x + int(w * 0.25)
                mouth_x_end = x + int(w * 0.75)
                mouth_y_start = y + int(h * 0.5)
                mouth_y_end = y + h
                mouth_roi = rgb_frame[mouth_y_start:mouth_y_end, mouth_x_start:mouth_x_end]

                if mouth_roi.size == 0:
                    continue

                try:
                    emotion_result = DeepFace.analyze(
                        img_path=mouth_roi,
                        actions=['emotion'],
                        enforce_detection=False,
                        detector_backend='opencv'
                    )[0]

                    emotion = emotion_result['dominant_emotion']
                    confidence = emotion_result['emotion'][emotion]

                    log_writer.writerow([
                        round(timestamp, 2),
                        frame_number,
                        emotion,
                        round(confidence, 1)
                    ])

                    emotion_stats[emotion]['count'] += 1
                    emotion_stats[emotion]['total_confidence'] += confidence

                    # Show box + label (on full frame)
                    cv2.rectangle(frame, (x, mouth_y_start), (x + w, mouth_y_end), (0, 255, 0), 2)
                    label = f"{emotion} ({confidence:.1f}%)"
                    cv2.putText(frame, label, (x, mouth_y_start - 10),
                                cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0), 2)

                except Exception as e:
                    print("DeepFace error:", e)
                    continue

            cv2.imshow('Mouth Emotion Analysis', frame)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

        for emotion, stats in emotion_stats.items():
            avg_conf = stats['total_confidence'] / stats['count']
            summary_writer.writerow([emotion, stats['count'], round(avg_conf, 1)])

    cap.release()
    cv2.destroyAllWindows()
    print("Processing complete. Logs saved.")

if __name__ == "__main__":
    video_path = "/Users/johnbacho/Desktop/Movie on 4-10-25 at 5.52 PM.mov"
    analyze_video_emotions(video_path)
