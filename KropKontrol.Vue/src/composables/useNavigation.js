import { useRouter } from "vue-router";

export function useNavigation() {
  const router = useRouter();

  function goLanding() {
    router.push({ name: "Landing" });
  }

  function goSnapshot() {
    router.push({ name: "Snapshot" });
  }

  return { goLanding, goSnapshot };
}

export default useNavigation;
