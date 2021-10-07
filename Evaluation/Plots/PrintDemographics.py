import pandas as pd

data = pd.read_csv('../Data/Demographics.csv')

print('Country:')
print(data['Country'].value_counts())
print()

print('Sex:')
print(data['Sex'].value_counts())
print()

print('Age:')
print(data['Age'].value_counts())
print()

print('VR Experience:')
print(data['VR Experience'].value_counts())
print()

print('Skin Tone:')
print(data['Skin Tone'].value_counts())
print()

print('Device:')
print(data['Device'].value_counts())
print()
