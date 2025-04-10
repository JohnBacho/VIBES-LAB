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
    frame_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
    frame_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))

    with open('emotion_log.csv', 'w', newline='') as log_file, \
         open('emotion_summary.csv', 'w', newline='') as summary_file:
        
        log_writer = csv.writer(log_file)
        summary_writer = csv.writer(summary_file)
        
        log_writer.writerow(['Timestamp (s)', 'Frame', 'Face', 'Emotion', 
                           'Confidence (%)', 'X', 'Y', 'Width', 'Height'])
        summary_writer.writerow(['Emotion', 'Count', 'Average Confidence (%)'])

        frame_number = 0
        
        # Process video frames
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break

            frame_number += 1
            timestamp = frame_number / fps

            # Convert frame to RGB (DeepFace uses RGB format)
            rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

            try:
                # Analyze face and emotions
                results = DeepFace.analyze(
                    img_path=rgb_frame,
                    actions=['emotion'],
                    enforce_detection=False,
                    detector_backend='opencv'
                )
            except ValueError:
                continue

            # Process each detected face
            for face_id, face in enumerate(results):
                x = face['region']['x']
                y = face['region']['y']
                w = face['region']['w']
                h = face['region']['h']
                emotion = face['dominant_emotion']
                confidence = face['emotion'][emotion]

                # Write to log file
                log_writer.writerow([
                    round(timestamp, 2),
                    frame_number,
                    face_id + 1,
                    emotion,
                    round(confidence, 1),
                    x,
                    y,
                    w,
                    h
                ])

                emotion_stats[emotion]['count'] += 1
                emotion_stats[emotion]['total_confidence'] += confidence

                # Draw rectangle around face
                cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 255, 0), 2)
                
                # Display emotion text
                text = f"{emotion} ({confidence:.1f}%)"
                cv2.putText(frame, text, (x, y - 10),
                          cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)

            # Display the resulting frame
            cv2.imshow('Emotion Analysis', frame)

            # Press 'q' to exit
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

        # Write summary statistics
        for emotion, stats in emotion_stats.items():
            avg_confidence = stats['total_confidence'] / stats['count']
            summary_writer.writerow([
                emotion,
                stats['count'],
                round(avg_confidence, 1)
            ])

    # Release resources
    cap.release()
    cv2.destroyAllWindows()
    print("Processing complete. Data saved to emotion_log.csv and emotion_summary.csv")

if __name__ == "__main__":
    video_path = "/Users/johnbacho/Desktop/Emotions.mov"
    analyze_video_emotions(video_path)