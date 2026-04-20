import { Stack } from "expo-router";
import { useFonts } from "expo-font";
import { useEffect } from "react";
import * as SplashScreen from "expo-splash-screen";
import { HeroUINativeProvider } from "heroui-native";
import { GestureHandlerRootView } from "react-native-gesture-handler";
import '../globals.css';
import { 
  Lexend_100Thin,
  Lexend_200ExtraLight,
  Lexend_300Light,
  Lexend_400Regular, 
  Lexend_500Medium,
  Lexend_600SemiBold,
  Lexend_700Bold,
  Lexend_800ExtraBold,
  Lexend_900Black
} from "@expo-google-fonts/lexend";

export default function RootLayout() {
  const [loaded] = useFonts({
    "Lexend-Thin": Lexend_100Thin,
    "Lexend-ExtraLight": Lexend_200ExtraLight,
    "Lexend-Light": Lexend_300Light,
    "Lexend-Regular": Lexend_400Regular,
    "Lexend-Medium": Lexend_500Medium,
    "Lexend-SemiBold": Lexend_600SemiBold,
    "Lexend-Bold": Lexend_700Bold,
    "Lexend-ExtraBold": Lexend_800ExtraBold,
    "Lexend-Black": Lexend_900Black,
  })

  useEffect(() => {
    SplashScreen.preventAutoHideAsync().catch(() => { });
  }, []);

  useEffect(() => {
    if (loaded) {
      SplashScreen.hideAsync().catch(() => { });
    }
  }, [loaded]);

  if (!loaded) {
    return null;
  }

  return (
    <GestureHandlerRootView style={{ flex: 1 }}>
      <HeroUINativeProvider>
        <Stack>
          <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
        </Stack>
      </HeroUINativeProvider>
    </GestureHandlerRootView>
  );
}