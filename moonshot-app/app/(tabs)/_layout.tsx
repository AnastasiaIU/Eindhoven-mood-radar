import { Map, PartyPopper, Settings } from "lucide-react-native";
import { Tabs } from "expo-router";
import { View, Text } from "react-native";
import { useThemeColor } from "heroui-native";

export default function TabLayout() {
    const [accent] = useThemeColor(["accent"]);

    return (
        <Tabs screenOptions={{ tabBarActiveTintColor: accent }}>
            <Tabs.Screen
                name="index"
                options={{
                    tabBarLabel: ({ color }) => (
                        <Text className="text-xs font-normal" style={{ color }}>
                            Map
                        </Text>
                    ),
                    tabBarIcon: ({ color }) => <Map color={color} size={24} />,
                    headerLeft: () => (
                        <View className="flex-row w-4/5 ml-auto">
                            <Text className="text-3xl font-semibold">Map</Text>
                        </View>
                    ),
                    headerTitle: "",
                    headerStyle: { backgroundColor: "#f3f4f6" },
                    headerShadowVisible: false,
                }}
            />
            <Tabs.Screen
                name="events"
                options={{
                    tabBarLabel: ({ color }) => (
                        <Text className="text-xs font-normal" style={{ color }}>
                            Events
                        </Text>
                    ),
                    tabBarIcon: ({ color }) => <PartyPopper color={color} size={24} />,
                    headerLeft: () => (
                        <View className="flex-row w-4/5 ml-auto">
                            <Text className="font-semibold text-3xl">Events</Text>
                        </View>
                    ),
                    headerTitle: "",
                    headerStyle: { backgroundColor: "#f3f4f6" },
                    headerShadowVisible: false,
                }}
            />
            <Tabs.Screen
                name="settings"
                options={{
                    tabBarLabel: ({ color }) => (
                        <Text className="text-xs font-normal" style={{ color }}>
                            Settings
                        </Text>
                    ),
                    tabBarIcon: ({ color }) => <Settings color={color} size={24} />,
                    headerLeft: () => (
                        <View className="flex-row w-4/5 ml-auto">
                            <Text className="font-semibold text-3xl">Settings</Text>
                        </View>
                    ),
                    headerTitle: "",
                    headerStyle: { backgroundColor: "#f3f4f6" },
                    headerShadowVisible: false,
                }}
            />
        </Tabs>

    )
}