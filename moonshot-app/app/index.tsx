import { Text, View } from "react-native";
import { Button } from "heroui-native";

export default function Index() {
  return (
    <View className="flex-1 justify-center items-center">
      <Text className='text-xl font-bold mb-6'>Index Page</Text>
      <Button onPress={() => console.log('Pressed!')}>Get Started</Button>
    </View>
  );
}
