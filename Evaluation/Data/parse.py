import json
from datetime import datetime
import pandas as pd

def load_json(filename):
	with open(filename, 'r') as f:
		jdata  = json.load(f)

	data = []

	# good ol O(n)
	def _participated_already(r):
		for d in data:
			if d['PID'] == r['PID']:
				return True
		return False

	def _is_pilot(r):
		return int(row['start']) < 1620345953

	for row in jdata:
		# actually this only entails 1 participant
		if _participated_already(row):
			continue

		if _is_pilot(row):
			continue

		for answer in row['answers']:
			data.append({
				'PID': row['PID'],
				'Start': datetime.utcfromtimestamp(int(row['start'])),
				'End': datetime.utcfromtimestamp(int(row['end'])),
				'Device': row['device'],
				'DeviceID': row['deviceID'],
				'Country': row['country'],
				'Trial': answer['trial'],
				'Question': answer['name'],
				'Answer': answer['button_value'],
				'Round': answer['round_no'],
				'Condition': answer['condition']
			})
	return pd.DataFrame(data)
	
def save_demographics(data):
	data = data[data['Trial'] == 0]
	data = data[['PID', 'Start', 'Question', 'Answer', 'Device','DeviceID', 'Country']]
	data = pd.pivot_table(data, index=['PID', 'Start', 'Device', 'DeviceID', 'Country'], columns='Question', values='Answer')
	data = data.drop(columns=['ready-Q0', 'start-Q0'])
	data = data.rename(columns={
		'demographics-Q0': 'Age',
		'demographics-Q1': 'Sex',
		'demographics-Q2': 'VR Experience',
        'fitzpatrick-Q0': 'Skin Tone'
	})
	data['Age'] = data['Age'].map({1: '18-24', 2: '25-29', 3: '30-35', 4: '35-39', 5: '40-49', 6: '50-59', 7: '60-69'})
	data['Sex'] = data['Sex'].map({1: 'Male', 2: 'Female', 3: 'Non-Binary', 4: 'Prefer not to say'})
	data['VR Experience'] = data['VR Experience'].map({1: '0-3 hours', 2: '5-9 hours', 3: '10-49 hours', 4: '50-99 hours', 5: '100+ hours'})
	
	data = data.reset_index()
	data.to_csv('Demographics.csv', index=False)

def save_responses(data):
	data = data[data['Trial'] > 0]
	data = data[['PID', 'Condition', 'Trial', 'Question', 'Answer']]
	data['Answer'] = data['Answer'] - 4 # go from button ID to -3 to 3 scale
	data = pd.pivot_table(data, index=['PID', 'Trial', 'Condition'], columns='Question', values='Answer')
	
	data = data.rename(columns={
		'agency-Q0': 'Agency',
		'features-Q0': 'Resemblance',
		'mybody-Q0': 'Body Ownership',
		'humanness-Q0' : 'HQ0',
		'humanness-Q1' : 'HQ1',
		'humanness-Q2' : 'HQ2',
		'humanness-Q3' : 'HQ3',
		'humanness-Q4' : 'HQ4'
	})
	data['Humanness'] = (data['HQ0'] + data['HQ1'] + data['HQ2'] + data['HQ3'] + data['HQ4']) / 5.
	data = data.reset_index()
	data['Condition'] = data['Condition'].map({'HandAlien': 'Alien Hand', 'HandMatch': 'Matched Hand', 'HandMismatch': 'Mismatched Hand'})
	data = pd.melt(data, id_vars=['PID', 'Trial', 'Condition'], var_name='Measure', value_name='Response')
	data.to_csv('Responses.csv', index=False)

if __name__ == '__main__':
	data = load_json('data.json')
	save_demographics(data)
	save_responses(data)