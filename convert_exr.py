import sys
import os
import glob
import argparse

# This environment variable MUST be set before cv2 is imported
os.environ["OPENCV_IO_ENABLE_OPENEXR"] = "1"

import cv2
import numpy as np

def convert_exr_to_png(input_path, output_path, exposure=1.0, gamma=2.2):
    """Converts EXR to PNG with exposure and gamma correction."""
    # Load the EXR (32-bit float)
    img = cv2.imread(input_path, cv2.IMREAD_UNCHANGED)

    if img is None:
        print(f"Error: Could not load {input_path}")
        return

    # 1. Apply Manual Exposure (Multiplier)
    # Increasing this makes the image brighter
    img = img * exposure

    # 2. Apply Gamma Correction 
    # This transforms linear data into a space that looks natural on monitors
    img = np.clip(img, 0, None) # Ensure no negative values
    img = np.power(img, 1.0 / gamma)

    # 3. Normalize and Convert to 8-bit
    # We use 255.0 to map the 0.0-1.0 range to 0-255
    img_8bit = np.clip(img * 255, 0, 255).astype(np.uint8)

    cv2.imwrite(output_path, img_8bit)
    print(f"Converted: {os.path.basename(input_path)} (Exp: {exposure}, Gamma: {gamma})")

def process_batch(input_folder, output_folder, exposure, gamma):
    if not os.path.exists(output_folder):
        os.makedirs(output_folder)

    files = glob.glob(os.path.join(input_folder, "*.exr")) + glob.glob(os.path.join(input_folder, "*.EXR"))

    if not files:
        print(f"No EXR files found in {input_folder}")
        return

    for file_path in files:
        file_name = os.path.splitext(os.path.basename(file_path))[0] + ".png"
        save_path = os.path.join(output_folder, file_name)
        convert_exr_to_png(file_path, save_path, exposure, gamma)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Batch convert EXR to PNG with brightness control.")
    parser.add_argument("input", help="Input file or folder")
    parser.add_argument("output", nargs="?", help="Output file or folder")
    parser.add_argument("--exposure", type=float, default=1.0, help="Brightness multiplier (e.g., 2.0 to double brightness)")
    parser.add_argument("--gamma", type=float, default=2.2, help="Gamma correction (default 2.2 for sRGB)")

    args = parser.parse_args()

    # Determine if we are doing batch or single file
    if os.path.isdir(args.input):
        if not args.output:
            print("Error: Please provide an output folder for batch processing.")
        else:
            process_batch(args.input, args.output, args.exposure, args.gamma)
    else:
        out_path = args.output if args.output else os.path.splitext(args.input)[0] + ".png"
        convert_exr_to_png(args.input, out_path, args.exposure, args.gamma)