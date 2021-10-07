import os
import json
import numpy as np
import pandas as pd
import matplotlib as mpl
import matplotlib.pyplot as plt
import matplotlib.collections
import matplotlib.lines

pd.set_option('display.max_rows', None)
pd.set_option('display.max_columns', None)
pd.set_option('display.width', None)
pd.set_option('display.max_colwidth', None)

# https://www.frontiersin.org/about/author-guidelines#FigureTableGuidelines
# For reference: https://github.com/RainCloudPlots/RainCloudPlots

data = pd.read_csv('../Data/Responses.csv')
data = data.groupby(['PID', 'Condition', 'Measure'])['Response'].median().reset_index()

conditionOrder = ['Alien Hand', 'Mismatched Hand', 'Matched Hand']
measureOrder = ['Body Ownership', 'Agency', 'Resemblance', 'Humanness']

#for font in mpl.font_manager.fontManager.ttflist:
#	print(font)

#mpl.rcParams['font.family'] = 'Comic Sans MS'
#mpl.rcParams['font.family'] = 'Roboto Medium'
mpl.rcParams['font.family'] = 'Open Sans'
mpl.rcParams['axes.spines.right'] = False
mpl.rcParams['axes.spines.top'] = False

titledict = dict(fontsize=12)
labeldict = dict(fontsize=10)


fig, axes = plt.subplots(2, 2, sharex=True, sharey=True, figsize=(7.09, 3.5), dpi=300)
for i, ax in enumerate(axes.flat):
	adata = data[data['Measure'] == measureOrder[i]]
	adata = [adata[adata['Condition'] == c]['Response'] for c in conditionOrder]

	v = ax.violinplot(adata, showmeans=False, showextrema=False)
	# https://stackoverflow.com/questions/29776114/half-violin-plot-in-matplotlib/29781988#29781988
	for b in v['bodies']:
		# get the center
		m = np.mean(b.get_paths()[0].vertices[:, 0])
		# modify the paths to not go further left than the center
		b.get_paths()[0].vertices[:, 0] = np.clip(b.get_paths()[0].vertices[:, 0], m, np.inf)
		# shift violin slightly to the right
		b.get_paths()[0].vertices[:, 0] += 0.03
		b.set_color('#3498db')
		b.set_alpha(1.0)

	bp = ax.boxplot(adata, vert=True, widths=0.15, patch_artist=True, showcaps=False, showfliers=False, 
					positions=np.arange(0, len(conditionOrder), 1) + 0.9, medianprops=dict(color='k'))
	for patch in bp['boxes']:
		patch.set_facecolor('#3498db')

	ax.set_title(measureOrder[i], fontdict=titledict)
	ax.set_xlim(0.7, 3.4)
	ax.set_xticks([1, 2, 3])
	ax.set_xticklabels(conditionOrder, fontdict=labeldict)
	ax.set_ylim(-3.2, 3.2)
	ax.set_yticks([-3, -2, -1, 0, 1, 2, 3])
	ax.set_yticklabels(['-3', '', '', '', '', '', '3'], fontdict=labeldict)
    
axes[0, 0].set_ylabel('Response', labelpad=-4, fontdict=labeldict)
axes[1, 0].set_ylabel('Response', labelpad=-4, fontdict=labeldict)

plt.tight_layout(pad=0.2, w_pad=0.1, h_pad=0.3)
plt.savefig('ResponsesRaincloudplot.pdf', dpi=300)

plt.show()
