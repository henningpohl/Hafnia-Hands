import os
import json
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

pd.set_option('display.max_rows', None)
pd.set_option('display.max_columns', None)
pd.set_option('display.width', None)
pd.set_option('display.max_colwidth', None)


data = pd.read_csv('../Data/Responses.csv')
data = data.groupby(['PID', 'Condition', 'Measure'])['Response'].median().reset_index()

# Normalize back to -3 to 3 instead of summed up index
data.loc[data['Measure'] == 'Humanness', 'Response'] /= 5.0

conditionOrder = ['Alien Hand', 'Mismatched Hand', 'Matched Hand']
measureOrder = ['Body Ownership', 'Agency', 'Resemblance', 'Humanness']

sns.set_context('paper', font_scale=0.9)
#sns.set_style()

g = sns.catplot(x='Condition', y='Response', col='Measure', kind='box', data=data,
	 order=conditionOrder, col_order=measureOrder, height=2.5, aspect=1.4)
g.set_axis_labels('', 'Response')
g.set_titles('{col_name}')
g.set(ylim=(-3, 3))

plt.tight_layout(pad=0.2)
plt.savefig('ResponsesBoxplot.pdf', dpi=300)

g = sns.catplot(x='Condition', y='Response', col='Measure', kind='bar', data=data,
	 order=conditionOrder, col_order=measureOrder, height=2.5, aspect=1.4)
g.set_axis_labels('', 'Response')
g.set_titles('{col_name}')
g.set(ylim=(-3, 3))

plt.tight_layout(pad=0.2)
plt.savefig('ResponsesBarplot.pdf', dpi=300)


plt.show()
