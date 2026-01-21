import React, { useState, useEffect } from 'react';
import { v4 as uuidv4 } from 'uuid';
import Cookies from 'js-cookie';

const API_URL = 'http://localhost:5163/api/clients'; // Backend running on port 5163

function App() {
  const [clientId, setClientId] = useState(null);
  const [data, setData] = useState(null);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);

  // Set client ID cookie on first visit
  useEffect(() => {
    let id = Cookies.get('clientId');
    if (!id) {
      id = uuidv4();
      Cookies.set('clientId', id, { expires: 365 }); // Persistent cookie
    }
    setClientId(id);
  }, []);

  // Function to fetch data with polling
  const fetchData = async () => {
    if (!clientId) return;

    setLoading(true);
    setError(null);

    try {
      const response = await fetch(`${API_URL}?clientId=${clientId}`);

      if (response.status === 200) {
        const result = await response.text();
        setData(result);
        setLoading(false);
      } else if (response.status === 202) {
        // Poll again in 5 seconds
        setTimeout(fetchData, 5000);
      } else if (response.status === 500) {
        const errMsg = await response.text();
        setError(errMsg);
        setLoading(false);
      } else {
        setError('Unexpected error');
        setLoading(false);
      }
    } catch (err) {
      setError('Failed to fetch data');
      setLoading(false);
    }
  };

  // Auto-fetch on load if clientId is set
  useEffect(() => {
    if (clientId) {
      fetchData();
    }
  }, [clientId]);

  return (
    <div style={{ padding: '20px' }}>
      <h1>Long-Running Data Fetcher</h1>
      <p>Client ID: {clientId || 'Generating...'}</p>
      {loading && <p>Loading data... (may take up to 60 seconds)</p>}
      {data && <p>Data: {data}</p>}
      {error && <p>Error: {error}</p>}
      {!loading && !data && !error && (
        <button onClick={fetchData}>Fetch Data</button>
      )}
    </div>
  );
}

export default App;