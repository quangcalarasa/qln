importScripts('https://www.gstatic.com/firebasejs/9.3.0/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/9.3.0/firebase-messaging-compat.js');

firebase.initializeApp({
  apiKey: "AIzaSyDigbpq059nmCkuzvlUuWdgpi2OSTezbpg",
  authDomain: "qln-hcm.firebaseapp.com",
  projectId: "qln-hcm",
  storageBucket: "qln-hcm.appspot.com",
  messagingSenderId: "105442962473",
  appId: "1:105442962473:web:204b16daa568b2b4f80038",
  measurementId: "G-6C9G6034TN"
});

const messaging = firebase.messaging();

messaging.onBackgroundMessage(function (payload) {
    return payload;
});