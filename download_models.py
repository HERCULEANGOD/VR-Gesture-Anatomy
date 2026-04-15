import requests
import os

# BodyParts3D URLs (example - adjust to actual URLs)
models = {
    'heart.obj': 'http://lifesciencedb.jp/bp3d/download/heart.obj',
    'brain.obj': 'http://lifesciencedb.jp/bp3d/download/brain.obj',
    'lung.obj': 'http://lifesciencedb.jp/bp3d/download/lung.obj',
    'liver.obj': 'http://lifesciencedb.jp/bp3d/download/liver.obj'
}

save_dir = 'Assets/Models/'
os.makedirs(save_dir, exist_ok=True)

for name, url in models.items():
    try:
        response = requests.get(url)
        if response.status_code == 200:
            with open(os.path.join(save_dir, name), 'wb') as f:
                f.write(response.content)
            print(f"Downloaded {name}")
        else:
            print(f"Failed to download {name}: {response.status_code}")
    except Exception as e:
        print(f"Error downloading {name}: {e}")