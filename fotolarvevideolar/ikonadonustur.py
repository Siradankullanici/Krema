import imageio
from PIL import Image

# PNG dosyasını oku
png_image_path = "AcmakIsterMisin.png"
image = imageio.imread(png_image_path)

# Pil objesine dönüştür
pil_image = Image.fromarray(image)

# İkonu kaydet
ico_image_path = "AcmakIsterMisin.ico"
pil_image.save(ico_image_path, format='ICO')

print(f"{png_image_path} has been successfully converted to {ico_image_path}.")