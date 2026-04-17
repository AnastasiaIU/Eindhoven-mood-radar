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
                    title: "Map",
                    tabBarIcon: ({ color }) => <Map color={color} size={24} />,
                    headerLeft: () => (
                        <View className="flex-row w-4/5 ml-auto">
                            <Text className="font-bold text-3xl">Map</Text>
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
                    title: "Events",
                    tabBarIcon: ({ color }) => <PartyPopper color={color} size={24} />,
                    headerLeft: () => (
                        <View className="flex-row w-4/5 ml-auto">
                            <Text className="font-bold text-3xl">Events</Text>
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
                    title: "Settings",
                    tabBarIcon: ({ color }) => <Settings color={color} size={24} />,
                    headerLeft: () => (
                        <View className="flex-row w-4/5 ml-auto">
                            <Text className="font-bold text-3xl">Settings</Text>
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