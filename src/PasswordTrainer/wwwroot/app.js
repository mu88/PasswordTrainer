const form = document.getElementById("form");
const result = document.getElementById("result");
const submitButton = form.querySelector("button[type='submit']");

function toBase64(bytes) {
  let binary = '';
  for (let b of bytes) binary += String.fromCodePoint(b);
  return btoa(binary);
}

form.addEventListener("submit", async e => {
  e.preventDefault();
  result.textContent = "";

  submitButton.disabled = true;
  submitButton.textContent = "Checking…";

  const encoder = new TextEncoder();
  const pin = document.getElementById("pin").value;
  const id = document.getElementById("id").value;
  const password = document.getElementById("pw").value;

  const passwordBytes = encoder.encode(password);
  const passwordBase64 = toBase64(passwordBytes);

  const payload = {
    pin,
    id,
    password: passwordBase64
  };

  if (!payload.id || !payload.pin || !payload.password) {
    result.textContent = "⚠️ All fields are required";
    return;
  }

  try {
    const res = await fetch("/check", {
      method: "POST",
      headers: {"Content-Type": "application/json"},
      body: JSON.stringify(payload)
    });

    if (res.ok) {
      result.textContent = "✅ Correct";
    } else {
      result.textContent = "❌ Incorrect";
    }
  } catch (err) {
    result.textContent = "⚠️ Connection error";
    console.error(err);
  } finally {
    submitButton.disabled = false;
    submitButton.textContent = "Check Password";
    document.getElementById("pw").value = "";
    document.getElementById("id").value = "";
    document.getElementById("id").focus();
  }
});
