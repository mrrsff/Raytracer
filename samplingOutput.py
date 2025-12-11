from PIL import Image, ImageDraw
import math
import random
import sys

def multi_jittered_samples(n):
    s = int(math.sqrt(n))

    # Step 1 — create stratified grid with jittered centers
    samples = [[0.0, 0.0] for _ in range(n)]
    inv_s = 1.0 / s
    rng = random.Random()

    for i in range(s):
        for j in range(s):
            idx = i * s + j
            samples[idx][0] = (i + rng.random()) * inv_s
            samples[idx][1] = (j + rng.random()) * inv_s

    # Step 2 — permute X within each row
    for row in range(s):
        for i in range(s):
            k = rng.randint(i, s - 1)
            a = row * s + i
            b = row * s + k
            samples[a][0], samples[b][0] = samples[b][0], samples[a][0]

    # Step 3 — permute Y within each column
    for col in range(s):
        for j in range(s):
            k = rng.randint(j, s - 1)
            a = j * s + col
            b = k * s + col
            samples[a][1], samples[b][1] = samples[b][1], samples[a][1]

    return [(p[0], p[1]) for p in samples]

def uniform_samples(sampleCount):
    s = math.ceil(math.sqrt(sampleCount))
    cell = 1.0 / s
    samples = []
    for i in range(s):
        for j in range(s):
            if len(samples) >= sampleCount:
                break
            x = (j + 0.5) * cell
            y = (i + 0.5) * cell
            samples.append((x, y))
    return samples
def stratified_samples(sampleCount, seed=12345):
    s = int(math.sqrt(sampleCount))
    rng = random.Random(seed)
    samples = []

    # Grid size is 1/s
    inv_s = 1.0 / s

    for i in range(s):
        for j in range(s):
            # Offset by grid position (i, j) + random jitter
            x = (i + rng.random()) * inv_s
            y = (j + rng.random()) * inv_s
            samples.append((x, y))

    return samples
def random_samples(sampleCount, seed=12345):
    rng = random.Random(seed) # Seed ensures the same "mess" every time
    return [(rng.random(), rng.random()) for _ in range(sampleCount)]

def gaussian1d(x, sigma):
    inv2sigma2 = -0.5 / (sigma * sigma)
    return math.exp(x * x * inv2sigma2)

def gaussian(x, y):
    sigma = 0.5
    radius = sigma * 2
    if abs(x) > radius or abs(y) > radius:
        return 0.0

    gx = gaussian1d(x, sigma)
    gy = gaussian1d(y, sigma)
    return gx * gy


def visualize_sampling(cell_size, n, samples, filename):
    s = int(math.sqrt(n))
    
    padding = 4
    half_padding = padding // 2
    img = Image.new("RGB", (cell_size + padding, cell_size + padding), "white")
    draw = ImageDraw.Draw(img)

    # Draw grid borders in grey
    cw = cell_size / s
    ch = cell_size / s
    gray = (150, 150, 150)

    # # vertical borders
    # for i in range(s + 1):
    #     x = i * cw
    #     startX = x + half_padding
    #     draw.line((startX, half_padding, startX, cell_size + half_padding), fill=gray)
    # # horizontal borders
    # for j in range(s + 1):
    #     y = j * ch
    #     startY = y + half_padding
    #     draw.line((half_padding, startY, cell_size + half_padding, startY), fill=gray)

    point_radius = max(2, int(cell_size / (s * 32)))
    for x, y in samples:
        px = x * cell_size
        py = y * cell_size
        # weight = gaussian(x - 0.5, y - 0.5)
        weight = 1
        intensity = int((255 * (1.0 - weight)))
        fillColor = (intensity, intensity, intensity)  # Grayscale based on weight

        draw.ellipse((px - point_radius + half_padding, py - point_radius + half_padding,
                      px + point_radius + half_padding, py + point_radius + half_padding),
                     fill=fillColor)

    img.save(filename)
    print("saved image to", filename)

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("usage: python samplingOutput.py <cell_size> <num_samples>")
        # Default for debugging
        cell_size = 512
        n = 16 # Perfect square for grid
    else:
        cell_size = int(sys.argv[1])
        n = int(sys.argv[2])

    visualize_sampling(cell_size, n, random_samples(n, seed=42), "random_samples.png")

    visualize_sampling(cell_size, n, stratified_samples(n, seed=42), "stratified_samples.png")

    visualize_sampling(cell_size, n, multi_jittered_samples(n), "multi_jittered_samples.png")

    visualize_sampling(cell_size, n, uniform_samples(n), "uniform_samples.png")
    
    visualize_sampling(128, 1, uniform_samples(1), "no_samples.png")
    