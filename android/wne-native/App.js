import React, { useState } from 'react';
import { StyleSheet, Text, View, TouchableOpacity, FlatList, TextInput, ActivityIndicator, SafeAreaView, Platform } from 'react-native';
import axios from 'axios';
import MapView, { Marker } from 'react-native-maps'; // Use native maps, not leaflet!

// Note: Android Emulator maps localhost to 10.0.2.2! Your ASP.NET API runs here.
// You must start your WildNatureExplorer.API backend as normal.
const API_URL = 'http://10.0.2.2:5000/api'; 

export default function App() {
  const [currentScreen, setCurrentScreen] = useState('Home');

  return (
    <SafeAreaView style={styles.container}>
      {/* Universal Header */}
      <View style={styles.header}>
        {currentScreen !== 'Home' && (
          <TouchableOpacity onPress={() => setCurrentScreen('Home')} style={styles.backBtn}>
            <Text style={styles.backText}>{'< Back'}</Text>
          </TouchableOpacity>
        )}
        <Text style={styles.headerTitle}>Wild Nature (Native)</Text>
      </View>

      <View style={styles.content}>
        {currentScreen === 'Home' && (
          <HomeScreen onNavigate={(screen) => setCurrentScreen(screen)} />
        )}
        {currentScreen === 'Search' && <SearchScreen />}
        {currentScreen === 'Map' && <LiveMapScreen />}
      </View>
    </SafeAreaView>
  );
}

// -----------------------
// HOME SCREEN
// -----------------------
function HomeScreen({ onNavigate }) {
  return (
    <View style={styles.screenContainer}>
      <Text style={styles.heroText}>Welcome</Text>
      <Text style={styles.subheroText}>Native Android Experience</Text>

      <TouchableOpacity style={styles.card} onPress={() => onNavigate('Search')}>
        <Text style={styles.cardTitle}>Species Database</Text>
        <Text style={styles.cardDesc}>Uses Axios to fetch natively from your ASP.NET backend.</Text>
      </TouchableOpacity>

      <TouchableOpacity style={styles.card} onPress={() => onNavigate('Map')}>
        <Text style={styles.cardTitle}>Live Safari Map</Text>
        <Text style={styles.cardDesc}>Uses Google Maps API natively on Android instead of Leaflet.</Text>
      </TouchableOpacity>
    </View>
  );
}

// -----------------------
// SEARCH SCREEN
// -----------------------
function SearchScreen() {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(false);

  const handleSearch = async () => {
    if (!query) return;
    setLoading(true);
    try {
      // Connect to your exact ASP.NET backend
      const response = await axios.get(`${API_URL}/Species/search?query=${query}`);
      setResults(response.data);
    } catch(err) {
      console.error("Backend fetch error: ", err);
      // Fallback dummy data if you haven't booted the backend yet
      setResults([
        { id: 1, commonName: 'African Elephant', latinName: 'Loxodonta africana', conservationStatus: 'Vulnerable' },
        { id: 2, commonName: 'Giraffe', latinName: 'Giraffa camelopardalis', conservationStatus: 'Vulnerable' }
      ]);
    }
    setLoading(false);
  };

  return (
    <View style={styles.screenContainer}>
      <View style={styles.searchRow}>
        <TextInput
          style={styles.searchInput}
          placeholder="Search by name..."
          placeholderTextColor="#9ba998"
          value={query}
          onChangeText={setQuery}
          onSubmitEditing={handleSearch}
        />
        <TouchableOpacity style={styles.goBtn} onPress={handleSearch}>
          <Text style={styles.goText}>Go</Text>
        </TouchableOpacity>
      </View>

      {loading ? (
        <ActivityIndicator size="large" color="#d4af37" style={{marginTop: 50}} />
      ) : (
        <FlatList
          data={results}
          keyExtractor={(item) => item.id.toString()}
          style={{ marginTop: 20 }}
          renderItem={({ item }) => (
            <View style={styles.resultItem}>
              <Text style={styles.resName}>{item.commonName}</Text>
              <Text style={styles.resLatin}>{item.latinName}</Text>
              <Text style={styles.resStatus}>Status: {item.conservationStatus}</Text>
            </View>
          )}
          ListEmptyComponent={<Text style={styles.emptyText}>No species found yet.</Text>}
        />
      )}
    </View>
  );
}

// -----------------------
// MAP SCREEN
// -----------------------
function LiveMapScreen() {
  return (
    <View style={styles.mapContainer}>
      <MapView 
        style={styles.mapElement}
        initialRegion={{
          latitude: -2.333333,
          longitude: 34.833333,
          latitudeDelta: 10,
          longitudeDelta: 10,
        }}
      >
        <Marker 
          coordinate={{ latitude: -2.33, longitude: 34.83 }}
          title="Elephant Herd"
          description="Spotted moving slowly."
        />
        <Marker 
          coordinate={{ latitude: -1.2921, longitude: 36.8219 }}
          title="Lion Pride"
          description="Resting near Nairobi"
        />
      </MapView>
    </View>
  );
}

// -----------------------
// NATIVE STYLES
// -----------------------
const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#0d1a10', // Deep wild nature forest color
    paddingTop: Platform.OS === 'android' ? 25 : 0,
  },
  header: {
    padding: 15,
    backgroundColor: '#0a140c',
    borderBottomWidth: 1,
    borderBottomColor: '#2e4934',
    flexDirection: 'row',
    alignItems: 'center',
  },
  headerTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#d4af37',
  },
  backBtn: {
    marginRight: 15,
  },
  backText: {
    color: '#2b9348',
    fontSize: 16,
    fontWeight: 'bold',
  },
  content: {
    flex: 1,
  },
  screenContainer: {
    flex: 1,
    padding: 20,
  },
  heroText: {
    fontSize: 32,
    fontWeight: 'bold',
    color: '#d4af37',
    marginBottom: 5,
  },
  subheroText: {
    fontSize: 18,
    color: '#9ba998',
    marginBottom: 30,
  },
  card: {
    backgroundColor: '#1b3221',
    padding: 20,
    borderRadius: 8,
    marginBottom: 15,
    borderWidth: 1,
    borderColor: '#2e4934',
  },
  cardTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#e3ebd1',
    marginBottom: 8,
  },
  cardDesc: {
    color: '#9ba998',
    fontSize: 14,
    lineHeight: 20,
  },
  searchRow: {
    flexDirection: 'row',
    gap: 10,
  },
  searchInput: {
    flex: 1,
    backgroundColor: '#1b3221',
    color: '#e3ebd1',
    paddingHorizontal: 15,
    paddingVertical: 12,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#2e4934',
  },
  goBtn: {
    backgroundColor: '#d4af37',
    paddingHorizontal: 20,
    justifyContent: 'center',
    alignItems: 'center',
    borderRadius: 8,
  },
  goText: {
    color: '#1b3221',
    fontWeight: 'bold',
    fontSize: 16,
  },
  resultItem: {
    backgroundColor: '#1b3221',
    padding: 15,
    borderRadius: 8,
    marginBottom: 10,
  },
  resName: {
    color: '#d4af37',
    fontSize: 18,
    fontWeight: 'bold',
    marginBottom: 5,
  },
  resLatin: {
    color: '#e3ebd1',
    fontStyle: 'italic',
    marginBottom: 5,
  },
  resStatus: {
    color: '#9ba998',
  },
  emptyText: {
    color: '#9ba998',
    textAlign: 'center',
    marginTop: 30,
  },
  mapContainer: {
    flex: 1,
    backgroundColor: '#1b3221'
  },
  mapElement: {
    width: '100%',
    height: '100%',
  }
});
