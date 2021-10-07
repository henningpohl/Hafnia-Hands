import os
import base64
import json
import gzip
import random
import pandas as pd
import matplotlib.pyplot as plt
import mpl_toolkits.mplot3d 

def load(filename):
	with open(filename, 'r') as f:
		data = f.read()
	data = base64.b64decode(data)
	data = gzip.decompress(data)
	data = json.loads(data)
	return data

def plot(data):
	left = pd.DataFrame(data['LeftHandPositions'])
	right = pd.DataFrame(data['RightHandPositions'])


	plt.figure()
	ax = plt.gca(projection='3d')

	ax.plot(left['x'], left['y'], left['z'])
	ax.plot(right['x'], right['y'], right['z'])

	ax.set_xlabel('X')
	ax.set_ylabel('Y')
	ax.set_zlabel('Z')

	plt.show()

if __name__ == '__main__':
	files_to_plot = os.listdir('../Data/s3-data')
	random.shuffle(files_to_plot)
	data = load(os.path.join('../Data/s3-data', files_to_plot[0]))
	#print(data.keys())
	plot(data)
