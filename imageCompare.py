from PIL import Image
import numpy as np
import sys
import os


def compare_images(image1_path, image2_path, tolerance=5):
    img1 = Image.open(image1_path).convert("RGB")
    img2 = Image.open(image2_path).convert("RGB")

    if img1.size != img2.size:
        raise ValueError("Images must have the same dimensions!")

    arr1 = np.asarray(img1, dtype=np.float32)
    arr2 = np.asarray(img2, dtype=np.float32)

    diff = arr1 - arr2
    distance_sq = np.sum(diff ** 2, axis=-1)
    distance_sq = np.clip(distance_sq, 0, None)
    distance = np.sqrt(distance_sq)

    similar = np.sum(distance <= tolerance)
    total = distance.size
    similarity_percent = (similar / total) * 100.0

    diff_mask = distance > tolerance
    diff_visual = np.copy(arr1)
    diff_visual[diff_mask] = [255, 0, 0]
    diff_visual = np.uint8(np.clip(diff_visual, 0, 255))

    if similarity_percent > 99.0:
        return similarity_percent, "No significant differences detected."
    # Save diff image next to original
    base1 = os.path.splitext(os.path.basename(image1_path))[0]
    base2 = os.path.splitext(os.path.basename(image2_path))[0]
    diff_name = f"diff_{base1}_vs_{base2}.png"
    Image.fromarray(diff_visual).save(diff_name)

    return similarity_percent, diff_name


if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python imageCompare.py <image1> <image2>")
        sys.exit(1)

    img1 = sys.argv[1]
    img2 = sys.argv[2]

    similarity, diff_path = compare_images(img1, img2)
    print(f"Images are {similarity:.2f}% similar.")
    print(f"Visual diff saved as: {diff_path}")
